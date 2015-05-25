using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBlocks.Messaging.Common;

namespace ServiceBlocks.Messaging.Tests.Common
{
    public class MockSnapshotClient : SnapshotClient<IList<MockMessage>>
    {
        public MockSnapshotClient()
            : this(GenerateMessages)
        {

        }

        public MockSnapshotClient(Func<byte[], IList<MockMessage>> deserializer)
            : base(deserializer)
        {
        }

        public override byte[] GetSnapshot(string topic)
        {
            return Data.SelectMany(m => m.Data).ToArray();
        }

        public IEnumerable<MockMessage> Data { get; set; }

        internal static IList<MockMessage> GenerateMessages(byte[] data)
        {
            var result = new List<MockMessage>();
            for (int i = 0; i < data.Length; i += 2)
            {
                result.Add(new MockMessage() { Data = new[] { data[i], data[i + 1] } });
            }
            return result;
        }
    }
}
