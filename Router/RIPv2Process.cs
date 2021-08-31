using System;
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
        private ConcurrentDictionary<int, RIPv2Entry> addedNetworks;
        private ConcurrentQueue<int> tickets;
        private RIPv2Handler RIPHandler;
        private int id;
        private object locker;

        public RIPv2Process(RIPv2Handler h)
        {
            tickets = new ConcurrentQueue<int>();
            addedNetworks = new ConcurrentDictionary<int, RIPv2Entry>();
            id = 0;
            locker = new object();
            RIPHandler = h;
        }

        public void Add(RIPv2Entry e)
        {
            int i;
            if (tickets.TryDequeue(out i))
                addedNetworks.TryAdd(i, e);
            else
            {
                lock (locker)
                {
                    addedNetworks.TryAdd(id, e);
                    id++;
                }
                
                if (RIPHandler.Sender1.Sending)
                {
                    if (!(IpV4.ToNetworkAdress(RIPHandler.Sender1.Rp.Ip, RIPHandler.Sender1.Rp.Mask) == e.Ip))
                        RIPHandler.Sender1.SendAddedInfo(e);
                }
                else if (RIPHandler.Sender2.Sending)
                {
                    if (!(IpV4.ToNetworkAdress(RIPHandler.Sender2.Rp.Ip, RIPHandler.Sender2.Rp.Mask) == e.Ip))
                        RIPHandler.Sender2.SendAddedInfo(e);
                }
            }

        }

        public void Delete(int id)
        {
            try
            {
                var en = addedNetworks[id];
                if (RIPHandler.Sender1.Sending)
                {
                    RIPHandler.Sender1.SendRemovedInfo(en);
                }
                else if (RIPHandler.Sender2.Sending)
                {
                    RIPHandler.Sender2.SendRemovedInfo(en);
                }
                addedNetworks.TryRemove(id, out _);
                tickets.Enqueue(id);
            }
            catch (Exception)
            {
            }
        }

        public void DeleteConnected(Router r)
        {
            List<int> toRemove = new List<int>(2);
            for (int i = 0; i < id; i++)
            {
                RIPv2Entry e;
                if (addedNetworks.TryGetValue(i, out e))
                {
                    if (e.Ip == IpV4.ToNetworkAdress(r.Port1.Ip, r.Port1.Mask) || e.Ip == IpV4.ToNetworkAdress(r.Port2.Ip, r.Port2.Mask))
                    {
                        toRemove.Add(i);
                    }
                }
            }
            foreach (var re in toRemove)
            {
                Delete(re);
            }
        }

        public bool IsInProcess(IpV4Address netIp)
        {
            var e = GetAddedNetworks();
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
            var e = GetAddedNetworks();
            RIPv2EntryOrdered res;

            while (e.TryDequeue(out res))
            {
                if (ipp == res.Ip) return true;
            }

            return false;
        }

        public ConcurrentQueue<RIPv2EntryOrdered> GetAddedNetworks()
        {
            ConcurrentQueue<RIPv2EntryOrdered> q = new ConcurrentQueue<RIPv2EntryOrdered>();
            Parallel.For(0, id, i => {
                RIPv2Entry e;
                if (addedNetworks.TryGetValue(i, out e))
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
