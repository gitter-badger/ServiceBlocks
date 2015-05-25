using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBlocks.Messaging.Common;

namespace ServiceBlocks.Messaging.Tests.Common
{
    public class MockTopicSubscriber : TopicSubscriber
    {
        public void InvokeSubscriptionAccessor(string topic, byte[] body)
        {
            InvokeSubscription(topic, body);
        }

        public IEnumerable<string> GetTopicsAccessor()
        {
            return GetTopics();
        }

        public bool IsEmptyAccessor()
        {
            return IsEmpty;
        }
    }
}
