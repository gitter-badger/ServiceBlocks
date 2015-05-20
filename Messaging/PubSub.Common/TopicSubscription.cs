using System;

namespace ServiceBlocks.Messaging.Common
{
    public class TopicSubscription<T> : ITopicSubscription
        where T : class
    {
        public Action<T> MessageHandler { get; set; }
        public Func<byte[], T> Deserializer { get; set; }

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