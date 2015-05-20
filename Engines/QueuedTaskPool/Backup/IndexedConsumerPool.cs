using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using iFOREX.Utilities.Threading;

namespace iForex.Trading.LimitMonitor.Core.Coordinators
{
    public class IndexedConsumerPool<TKey, TItem> : IDisposable
        where TKey : IEquatable<TKey>
    {
        private readonly int _waitForCompletionTimout = 60000;
        private readonly int _maxDegreeOfParallelism = 100;
        private readonly bool _suspendedConsumersInPool = true;

        private ConcurrentDictionary<TKey, IConsumer<TKey, TItem>> _index;
        private ConcurrentBag<IConsumer<TKey, TItem>> _consumersPool = new ConcurrentBag<IConsumer<TKey, TItem>>();
        private BlockingCollection<TKey> _pendingRequests = new BlockingCollection<TKey>();

        private Action<Exception> _onErrorAction;
        private Func<Action<TItem>> _consumeActionFactory;
        private Func<Action<TItem>> _onAddActionFactory;
        private TaskFactory _taskFactory;

        private object _indexLocker = new object();

        public IndexedConsumerPool(Func<Action<TItem>> consumeActionFactory, Func<Action<TItem>> onAddActionFactory = null, Action<Exception> onErrorAction = null,
            int maxDegreeOfParallelism = 100, bool suspendCompletedConsumer = true, int waitForCompletionTimout = 60000)
        {
            if (consumeActionFactory == null)
                throw new ArgumentNullException("consumerActionFactory", "consumerActionFactory cannot be null. Please provide a valid factory function.");

            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _suspendedConsumersInPool = suspendCompletedConsumer;
            _waitForCompletionTimout = waitForCompletionTimout;
            _consumeActionFactory = consumeActionFactory;
            _onAddActionFactory = onAddActionFactory ?? (() => { return null; });
            _onErrorAction = onErrorAction ?? ((ex) => Debug.Write(ex.ToString()));
            _index = new ConcurrentDictionary<TKey, IConsumer<TKey, TItem>>();
            StartTasks();
        }

        //TODO: Expose as PERF counters when used in real implementation
        public int IndexSize { get { return _index.Count; } }
        public int PoolSize { get { return _consumersPool.Count; } }
        public int PendingRequests { get { return _pendingRequests.Count; } }

        public void Add(TKey key, TItem item)
        {
            while (!TryAddInternal(key, item))
            {
                SpinWaitHelper.SpinWaitForTimeout(1);
                //continue;

                IConsumer<TKey, TItem> completedConsumer;

                if (_index.TryGetValue(key, out completedConsumer) && !completedConsumer.IsPending)
                {
                    bool consumerSwitched = false;
                    IConsumer<TKey, TItem> pendingConsumer = new PendingConsumer<TKey, TItem>(key);
                    lock (_indexLocker)
                    {
                        consumerSwitched = _index.TryUpdate(key, pendingConsumer, completedConsumer);
                    }
                    
                    if (!consumerSwitched)
                        Debug.WriteLine(string.Format("Failed to switch Pending Consumer. CompletedConsumer:{0} PendingConsumer:{1}", completedConsumer.GetHashCode(), pendingConsumer.GetHashCode()));
                    else
                        Debug.WriteLine(string.Format("Pending Consumer Switched. CompletedConsumer:{0} PendingConsumer:{1}", completedConsumer.GetHashCode(), pendingConsumer.GetHashCode()));
                }
                else
                    Debug.WriteLine(string.Format("Completed consumer not found!!!!"));
            }
        }

        private bool TryAddInternal(TKey key, TItem item)
        {
            bool consumerAdded = false;
            IConsumer<TKey, TItem> consumer;

            if (!_index.TryGetValue(key, out consumer))
            {
                lock (_indexLocker)
                {
                    if (!_index.TryGetValue(key, out consumer))
                    {
                        IConsumer<TKey, TItem> consumerFromPool;
                        if (_consumersPool.TryTake(out consumerFromPool))
                            consumer = consumerFromPool.SetKey(key).Start();
                        else
                            consumer = new PendingConsumer<TKey, TItem>(key);

                        consumerAdded = _index.TryAdd(key, consumer);
                    }
                }
            }

            if (consumerAdded)
                Debug.WriteLine(string.Format("Consumer Added Pending:{0} Key:{1} Item:{2} HashCode:{3}", consumer.IsPending, key, item, consumer.GetHashCode()));

            if (!consumer.TryAdd(item))
                return false;

            if (consumerAdded && consumer.IsPending)
                _pendingRequests.Add(key);

            return true;
        }

        public bool Exists(TKey key)
        {
            return _index.ContainsKey(key);
        }

        private void StartTasks()
        {
            _taskFactory = new TaskFactory(TaskScheduler.Default);

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
                _consumersPool.Add(CreateConsumer());

            _taskFactory.StartNew(ProcessPendingRequests, TaskCreationOptions.LongRunning);
        }

        private IConsumer<TKey, TItem> CreateConsumer()
        {
            return new RecyclableProducerConsumer<TKey, TItem>(_taskFactory, OnCompleted, !_suspendedConsumersInPool, (int)(_waitForCompletionTimout * 1.2)).
                        Init(_consumeActionFactory(), _onAddActionFactory(), _onErrorAction);
        }

        private void OnCompleted(IConsumer<TKey, TItem> completedConsumer)
        {
            IConsumer<TKey, TItem> consumerFromIndex;

            lock (_indexLocker)
            {
                if (_index.TryRemove(completedConsumer.Key, out consumerFromIndex))
                {
                    if (!consumerFromIndex.Equals(completedConsumer) && !_index.TryAdd(consumerFromIndex.Key, consumerFromIndex))
                        throw new ApplicationException("How Come Add failed after remove???");
                    else
                        Debug.WriteLine(string.Format("TryAdd Done Key:{0} consumer:{1}", consumerFromIndex.Key, consumerFromIndex.GetHashCode()));
                }
                else
                    Debug.WriteLine(string.Format("TryRemove Failed Key:{0} consumer:{1}", completedConsumer.Key, consumerFromIndex.GetHashCode()));

                //if (consumerFromIndex.IsPending)
                //    throw new ApplicationException("Pending Consumer Completed!!!");
            }

            Debug.WriteLine(string.Format("Releasing completed consumer. {0}", completedConsumer.GetHashCode()));

            try
            {

                if (_suspendedConsumersInPool) completedConsumer.Stop();
                _consumersPool.Add(completedConsumer);
            }
            catch (Exception ex)
            {
                //stop problematic consumer to complete its queue
                try { completedConsumer.Stop(); }
                catch (Exception cex) { LogError(cex); }

                //when exception occurs, consumer is missing in pool and new one should be created
                var newConsumer = CreateConsumer();
                _consumersPool.Add(newConsumer);

                LogError(ex);
            }
        }

        private void ProcessPendingRequests()
        {
            foreach (var key in _pendingRequests.GetConsumingEnumerable())
            {
                IConsumer<TKey, TItem> consumerFromPool;
                if (_consumersPool.TryTake(out consumerFromPool))
                {
                    IConsumer<TKey, TItem> pendingConsumer;
                    if (_index.TryGetValue(key, out pendingConsumer) && pendingConsumer.IsPending)
                    {
                        bool consumerSwitched = false;
                        lock (_indexLocker)
                        {
                            consumerFromPool.SetKey(key).Add(pendingConsumer.GetItems());
                            Debug.WriteLine(string.Format("TryUpdate Key:{0} consumerFromPool:{1} pendingConsumer:{2}", key, consumerFromPool.GetHashCode(), pendingConsumer.GetHashCode()));
                            consumerSwitched = _index.TryUpdate(key, consumerFromPool.Start(), pendingConsumer);
                        }

                        if (!consumerSwitched)
                            throw new ApplicationException("Failed to update pending consumer!!!!");
                        else
                            Debug.WriteLine(string.Format("Pending Consumer Switched. Pending:{0} Live:{1}", pendingConsumer.GetHashCode(), consumerFromPool.GetHashCode()));
                    }
                    else
                        throw new ApplicationException("Pending Consumer not found!!!!");
                }
                else
                {
                    SpinWaitHelper.SpinWaitForTimeout(10);
                    _pendingRequests.Add(key);
                }
            }
        }

        private void LogError(Exception ex)
        {
            if (_onErrorAction != null) _onErrorAction(ex);
        }

        #region IDisposable Members

        //try to dispose gracefully by allowing consumers to end the tasks
        public void Dispose()
        {
            SpinWaitHelper.SpinWaitForCondition(() => { return _pendingRequests.Count == 0; }, _waitForCompletionTimout * _pendingRequests.Count);

            _index.Clear();

            if (!_suspendedConsumersInPool)
            {
                IConsumer<TKey, TItem> consumer;
                while (_consumersPool.TryTake(out consumer))
                    consumer.Stop();
            }
        }

        #endregion
    }
}
