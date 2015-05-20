using System;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace iForex.Trading.LimitMonitor.Core.Coordinators
{
    public class RecyclableProducerConsumer<TKey, TItem> : IConsumer<TKey, TItem>, IDisposable
         where TKey : IEquatable<TKey>
    {
        private readonly int _waitForCompletionTimout;
        private BlockingCollection<TItem> _queue;
        private Action<TItem> _onAddAction;
        private Action<TItem> _consumeAction;
        private Action<Exception> _onErrorAction;
        private Action<IConsumer<TKey, TItem>> _onCompleted;

        private SpinLock _recycleLocker = new SpinLock();
        private TaskFactory _factory;
        private int _count = 0;
        private int _running = 0;
        private int _completed = 0;

        public bool IsRunning { get { return _running != 0; } }
        public TKey Key { get; private set; }
        public int Count { get { return _count; } }
        public bool IsPending { get { return false; } }

        public RecyclableProducerConsumer(TaskFactory factory = null, Action<IConsumer<TKey, TItem>> onCompleted = null, bool startTaskImmediately = false, int waitForCompletionTimout = 15000)
        {
            _waitForCompletionTimout = waitForCompletionTimout;
            _factory = factory ?? new TaskFactory();
            _onCompleted = onCompleted;
            _queue = new BlockingCollection<TItem>();
            if (startTaskImmediately) StartTask();
        }

        public RecyclableProducerConsumer<TKey, TItem> Init(Action<TItem> consumeAction, Action<TItem> onAddAction = null, Action<Exception> onErrorAction = null)
        {
            if (consumeAction == null)
                throw new ArgumentNullException("consumeAction", "consumeAction cannot be null. Please provide a valid delegate.");

            //interlock allows switching in runtime
            Interlocked.Exchange(ref _onErrorAction, onErrorAction);
            Interlocked.Exchange(ref _consumeAction, consumeAction);
            Interlocked.Exchange(ref _onAddAction, onAddAction);

            return this; //chain for fluent syntax
        }

        public IConsumer<TKey, TItem> SetKey(TKey key)
        {
            Debug.WriteLine(string.Format("SetKey. Old:{0} New:{1} HashCode:{2}", Key, key, this.GetHashCode()));
            Interlocked.CompareExchange(ref _completed, 0, 1);
            Key = key;
            return this;
        }

        public IConsumer<TKey, TItem> Start()
        {
            Interlocked.CompareExchange(ref _completed, 0, 1);
            StartTask();
            return this;
        }

        public void Stop()
        {
            Console.WriteLine("Stop Called:" + Key);
            if (!_queue.IsAddingCompleted)
            {
                bool lockTaken = false;
                try
                {
                    _recycleLocker.Enter(ref lockTaken);
                    if (!_queue.IsAddingCompleted)
                    {
                        _queue.CompleteAdding();
                        RecycleQueue();
                        Console.WriteLine("Stop Done:" + Key);
                    }
                }
                finally
                {
                    if (lockTaken) _recycleLocker.Exit(false);
                }
            }
        }

        public IConsumer<TKey, TItem> Add(IEnumerable<TItem> items)
        {
            Interlocked.Increment(ref _count);
            try
            {
                IEnumerator<TItem> cursor = items.GetEnumerator();
                TItem current;
                bool endLoop = false;
                if (cursor.MoveNext())
                    do
                    {
                        current = cursor.Current;
                        endLoop = !cursor.MoveNext();

                        if (endLoop)
                            Interlocked.Decrement(ref _count);

                        if (!TryAdd(current))
                            throw new ApplicationException("Failed to add from pending to completed consumer");

                    } while (!endLoop);

                return this;
            }
            catch (Exception)
            {
                if (!(Interlocked.CompareExchange(ref _count, 0, -1) == -1))
                    Interlocked.Decrement(ref _count);
                throw;
            }
        }

        public IEnumerable<TItem> GetItems()
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(TItem item)
        {
            Interlocked.Increment(ref _count);
            if (_completed != 0)
            {
                Interlocked.Decrement(ref _count);
                return false;
            }

            try { _queue.Add(item); }
            catch (Exception)
            {
                Interlocked.Decrement(ref _count);
                throw;
            }
            ItemAdded(item);
            return true;
        }

        private int StartTask()
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
            {
                var task = _factory.StartNew(Process, TaskCreationOptions.LongRunning).ContinueWith(t =>
                 {
                     Interlocked.Exchange(ref _running, 0);
                     Debug.WriteLine(string.Format("Task Completed: {0} TID:{1}", DateTime.UtcNow, t.Id));
                 });
                Debug.WriteLine(string.Format("Task Started: {0} TID:{1}", DateTime.UtcNow, task.Id));
                return task.Id;
            }

            return 0;
        }

        private BlockingCollection<TItem> RecycleQueue()
        {
            var newQueue = new BlockingCollection<TItem>();
            var oldQueue = Interlocked.Exchange(ref _queue, newQueue);
            return oldQueue;
        }

        private void ItemAdded(TItem item)
        {
            if (_onAddAction != null) _onAddAction(item);
            Debug.WriteLine(String.Format("Consumer: Add: {0} TID:{1}", item, Task.CurrentId.HasValue ? Task.CurrentId : Thread.CurrentThread.ManagedThreadId));
        }

        private void Process()
        {
            try
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        _consumeAction(item);
                        Interlocked.Decrement(ref _count);
                        // Debug.WriteLine(String.Format("Consumer: PerformAction: {0} TID:{1}", item, Task.CurrentId.HasValue ? Task.CurrentId : Thread.CurrentThread.ManagedThreadId));
                        if (_count == 0 && _onCompleted != null && Interlocked.CompareExchange(ref _completed, 1, 0) == 0)
                        {
                            if (_count == 0) //this check is safe to do after _completed set to 1
                                _onCompleted(this);
                            else
                                Interlocked.Exchange(ref _completed, 0); //if item added after completed, do not complete the consumer
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_onErrorAction != null)
                            _onErrorAction(ex);
                        else
                            throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                _onErrorAction(ex);
                _onCompleted(this);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region IEquatable<IConsumer<TKey,TItem>> Members

        public bool Equals(IConsumer<TKey, TItem> other)
        {
            return this.GetHashCode() == other.GetHashCode(); //this.Key.Equals(other.Key) && this.IsPending == other.IsPending;
        }

        #endregion





    }
}
