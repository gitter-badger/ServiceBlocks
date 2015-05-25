using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBlocks.Messaging.Common;

namespace ServiceBlocks.Messaging.Tests.Common
{
    public class MockBufferedSubscriber : BufferedSubscriber<MockMessage>
    {
        private readonly Action _producerAction;
        private readonly Action _consumeMessageAction;
        private readonly Action _consumeErrorAction;

        public MockBufferedSubscriber()
        {

        }

        public MockBufferedSubscriber(Action producerAction, Action consumeMessageAction, Action consumeErrorAction)
        {
            _producerAction = producerAction;
            _consumeMessageAction = consumeMessageAction;
            _consumeErrorAction = consumeErrorAction;
        }

        protected override void ProducerAction()
        {
            if (_producerAction != null)
                _producerAction();
        }

        protected override void ConsumeMessage(MockMessage message)
        {
            if (_consumeMessageAction != null)
                _consumeMessageAction();
        }

        protected override void ConsumeError(Exception ex)
        {
            if (_consumeErrorAction != null)
                _consumeErrorAction();
        }

        public void Publish(MockMessage message)
        {
            Queue.Add(message);
        }
    }
}
