using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBlocks.Engines.CommandProcessor;

namespace ServiceBlocks.CommandProcessor.Tests
{
    public class MockCommandProcessor : DefaultCommandProcessor<bool, int, MockCommand>
    {
    }
}
