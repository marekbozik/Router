using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    struct RIPv2Timer
    {
        private int update;
        private int invalid;
        private int holddown;
        private int flush;

        public int Update { get => update; set => update = value; }
        public int Invalid { get => invalid; set => invalid = value; }
        public int Holddown { get => holddown; set => holddown = value; }
        public int Flush { get => flush; set => flush = value; }
    }
}
