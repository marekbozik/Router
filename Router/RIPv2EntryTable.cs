using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2EntryTable
    {
        private byte[] raw;
        private List<RIPv2Entry> table;

        public RIPv2EntryTable(byte[] raw)
        {
            this.raw = raw;
            table = new List<RIPv2Entry>();
            int len = raw.Length / 20;
            int offset = 0;
            for (int i = 0; i < len; i++)
            {
                byte[] rec = new byte[20];
                int q = 0;
                for (int j = offset; j < offset + offset; j++)
                {
                    rec[q++] = raw[j]; 
                }
                table.Add(new RIPv2Entry(rec));
                
                offset += 20;
            }

        }

        public byte[] Raw { get => raw; set => raw = value; }
        internal List<RIPv2Entry> Table { get => table; set => table = value; }
    }
}
