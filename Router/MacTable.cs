using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class MacTable
    {
        private ConcurrentDictionary<MacAddress, MacLog> table;
        private int ageTimer;

        public int AgeTimer { get => ageTimer; set => ageTimer = value; }

        public MacTable(int ageTimer)
        {
            table = new ConcurrentDictionary<MacAddress, MacLog>();
            this.ageTimer = ageTimer;
        }

        public void UpdateTable(GenericPacket p, int port)
        {
            var z = table[p.SourceMacAddress];
            z.Mac = p.SourceMacAddress;
            z.Port = port;
            z.Time = DateTime.Now;
        }

        public void ClearTable() { table.Clear(); }

        //returns oldest mac or broadcast when table is empty
        public MacAddress CleanTable()
        {
            List<MacAddress> removeL = new List<MacAddress>();
            DateTime min = DateTime.Now;
            MacAddress last = new MacAddress("FF:FF:FF:FF:FF:FF");
            foreach (var i in table)
            {
                if ((DateTime.Now - i.Value.Time).TotalSeconds > ageTimer)
                {
                    removeL.Add(new MacAddress(i.Key.ToString()));
                }
                else if (i.Value.Time < min)
                {
                    min = i.Value.Time;
                    last = new MacAddress(i.Value.Mac.ToString());
                }
            }

            foreach (var i in removeL)
            {
                var x = table[i];
                table.TryRemove(i, out x);
            }

            return last;
        }

        public double LogAgeMiliseconds(MacAddress m)
        {
            double x = 0;
            try
            {
                x = (DateTime.Now - table[m].Time).TotalMilliseconds;
            }
            catch (Exception) { }

            return x;
        }

    }
}
