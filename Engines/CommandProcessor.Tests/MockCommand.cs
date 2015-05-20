using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBlocks.Engines.CommandProcessor;

namespace ServiceBlocks.CommandProcessor.Tests
{
    public class MockCommand : Command<bool>
    {

        public MockCommand()
            : base(true)
        {
        }

        public MockCommand(bool state)
            : base(state)
        {
        }

        protected override void ExecuteCommand()
        {
            State = false;
        }

        public bool CurrentState
        {
            get { return State; }
        }
    }
}
