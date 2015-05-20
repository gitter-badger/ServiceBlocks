using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iForex.Trading.LimitMonitor.Core.Coordinators
{
    public interface IConsumer<TKey, TItem> : IEquatable<IConsumer<TKey, TItem>>
        where TKey : IEquatable<TKey>
    {
        bool IsPending { get; }
        TKey Key { get; }
        IConsumer<TKey, TItem> SetKey(TKey key);
        bool TryAdd(TItem item);
        IConsumer<TKey, TItem> Add(IEnumerable<TItem> items);
        IEnumerable<TItem> GetItems();
        void Stop();
        IConsumer<TKey, TItem> Start();
        int Count { get; }

        
    }
}
