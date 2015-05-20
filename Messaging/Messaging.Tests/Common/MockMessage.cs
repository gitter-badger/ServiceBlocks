using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBlocks.Messaging.Tests.Common
{
    public class MockMessage
    {
        public string Topic { get; set; }
        public byte[] Data { get; set; }
    }
}
