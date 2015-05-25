using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceBlocks.Messaging.Tests.Common
{
    [TestClass]
    public class SnapshotClientTest
    {
        [TestMethod]
        public void Test_GetAndParseSnapshot()
        {
            var snapshotClient = new MockSnapshotClient()
            {
                Data = new[]{
                new MockMessage(){Data = new byte[]{1,11}},
                new MockMessage(){Data = new byte[]{2,22}},
                new MockMessage(){Data = new byte[]{3,33}}}
            };
            var result = snapshotClient.GetAndParseSnapshot("");
            Assert.AreEqual(1, result[0].Data[0]);
            Assert.AreEqual(2, result[1].Data[0]);
            Assert.AreEqual(3, result[2].Data[0]);
        }

    }
}
