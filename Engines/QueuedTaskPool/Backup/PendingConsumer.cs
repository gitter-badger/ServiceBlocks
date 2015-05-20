using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace iForex.Trading.LimitMonitor.Core.Coordinators
{
    public class PendingConsumer<TKey, TItem> : IConsumer<TKey, TItem>
        where TKey : IEquatable<TKey>
    {
        ConcurrentQueue<TItem> _queue = new ConcurrentQueue<TItem>();

        public PendingConsumer(TKey key)
        {
            Key = key;
        }

        #region IConsumer<TKey,TItem> Members

        public bool IsPending { get { return true; } }

        public IConsumer<TKey, TItem> Start()
        {
            throw new NotImplementedException();
        }

        public TKey Key { get; private set; }

        public IConsumer<TKey, TItem> SetKey(TKey key)
        {
            Key = key;
            return this;
        }

        public bool TryAdd(TItem item)
        {
            _queue.Enqueue(item);
            return true;
        }

        public IConsumer<TKey, TItem> Add(IEnumerable<TItem> items)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TItem> GetItems()
        {
            TItem item;
            while (_queue.TryDequeue(out item))
                yield return item;
            //return _queue.ToArray();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _queue.Count; }
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
