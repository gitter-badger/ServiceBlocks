using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace ServiceBlocks.CommandProcessor.Tests
{
    [TestClass]
    public class CommandTest
    {
        [TestMethod]
        public void TestExecuteCommand()
        {
            var mock = new Mock<MockCommand>();
            mock.Object.Execute();
            mock.Protected().Verify("ExecuteCommand", Times.Once());
            Assert.IsTrue(mock.Object.CreatedTime > DateTime.UtcNow.AddMinutes(-1));
            Assert.IsTrue(mock.Object.ExecuteStartedTime > mock.Object.CreatedTime);
            Assert.IsTrue(mock.Object.ExecuteCompletedTime > mock.Object.ExecuteStartedTime);
        }

        [TestMethod]
        public void TestExecuteCommandAsync()
        {
            var mock = new Mock<MockCommand>(true);
            var completed = mock.Object.Completed();
            mock.Object.Execute();
            var result = completed.Wait(500);
            mock.Protected().Verify("ExecuteCommand", Times.Once());
            Assert.IsTrue(result);
        }
    }
}
