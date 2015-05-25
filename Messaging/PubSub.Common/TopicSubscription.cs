using System;

namespace ServiceBlocks.Messaging.Common
{
    public class TopicSubscription<T> : ITopicSubscription
        where T : class
    {
        public Action<T> MessageHandler { get; set; }
        public Func<byte[], T> Deserializer { get; set; }

        public TopicSubscription()
        {
            Topic = typeof(T).FullName;
        }

        #region ITopicSubscription Members

        Delegate ITopicSubscription.MessageHandler
        {
            get { return MessageHandler; }
        }

        Func<byte[], object> ITopicSubscription.Deserializer
        {
            get { return Deserializer; }
        }

        #endregion

        public string Topic { get; set; }
    }
}