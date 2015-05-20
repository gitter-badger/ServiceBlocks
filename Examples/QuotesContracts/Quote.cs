using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuotesContracts
{
    public class Quote
    {
        public ushort InstrumentId { get; set; }

        public float Bid { get; set; }
        public float Ask { get; set; }
        public DateTime TradeTime { get; set; }
        public DateTime LastUpdate { get; set; }
        public int SourceId { get; set; }
    }
}
