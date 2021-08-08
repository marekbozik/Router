using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2EntryOrdered : RIPv2Entry
    {
        private int id;
        public RIPv2EntryOrdered(RIPv2Entry e, int id) : base(e)
        {
            this.id = id;
        }

        public int Id { get => id; set => id = value; }
    }
}
