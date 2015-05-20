using System;
using ServiceBlocks.Common.Threading;

namespace ServiceBlocks.Messaging.Common
{
    public class SubscriptionManager : ISubscriber, ITaskWorker, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string topic, Action<byte[]> messageHandler)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>(Action<T> messageHandler, Func<byte[], T> deSerializer) where T : class
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>(TopicSubscription<T> subscription) where T : class
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string topic)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop(int timeout)
        {
            throw new NotImplementedException();
        }
    }
}