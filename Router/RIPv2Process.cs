﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using PcapDotNet.Packets.IpV4;

namespace Router
{
    class RIPv2Process
    {
        private ConcurrentDictionary<int, RIPv2Entry> entries;
        private ConcurrentQueue<int> tickets;
        private int id;
        private object locker;

        public RIPv2Process()
        {
            tickets = new ConcurrentQueue<int>();
            entries = new ConcurrentDictionary<int, RIPv2Entry>();
            id = 0;
            locker = new object();
        }

        public void Add(RIPv2Entry e)
        {
            int i;
            if (tickets.TryDequeue(out i))
                entries.TryAdd(i, e);
            else
            {
                lock (locker)
                {
                    entries.TryAdd(id, e);
                    id++;
                }
            }

        }

        public void Delete(int id)
        {
            try
            {
                entries.TryRemove(id, out _);
                tickets.Enqueue(id);
            }
            catch (Exception)
            {
            }
        }

        public bool IsInProcess(IpV4Address netIp)
        {
            var e = GetEntries();
            RIPv2EntryOrdered res;

            while (e.TryDequeue(out res))
            {
                if (netIp == res.Ip) return true;
            }

            return false;
        }

        public bool IsInProcess(IpV4Address ip, string mask)
        {
            IpV4Address ipp = IpV4.ToNetworkAdress(ip, mask);
            var e = GetEntries();
            RIPv2EntryOrdered res;

            while (e.TryDequeue(out res))
            {
                if (ipp == res.Ip) return true;
            }

            return false;
        }

        public ConcurrentQueue<RIPv2EntryOrdered> GetEntries()
        {
            ConcurrentQueue<RIPv2EntryOrdered> q = new ConcurrentQueue<RIPv2EntryOrdered>();
            Parallel.For(0, id, i => {
                RIPv2Entry e;
                if (entries.TryGetValue(i, out e))
                {
                    try
                    {
                        q.Enqueue(new RIPv2EntryOrdered(e, i));
                    }
                    catch (Exception) { }
                }
            });
            return q;
        }

    }
}
