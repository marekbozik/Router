using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Router
{
    class RIPv2Process
    {
        private ConcurrentDictionary<int, bool> removed;
        private ConcurrentDictionary<int, RIPv2Entry> entries;
        private int id;
        private object locker;

        public RIPv2Process()
        {
            removed = new ConcurrentDictionary<int, bool>();
            entries = new ConcurrentDictionary<int, RIPv2Entry>();
            id = 0;
            locker = new object();
        }

        public void Add(RIPv2Entry e)
        {
            lock (locker)
            {
                entries.TryAdd(id, e);
                id++;
            }
        }

        public void Delete(int id)
        {
            try
            {
                removed.TryAdd(id, true);
                entries.TryRemove(id, out _);
            }
            catch (Exception)
            {
            }
        }

        public ConcurrentQueue<RIPv2EntryOrdered> GetEntries()
        {
            ConcurrentQueue<RIPv2EntryOrdered> q = new ConcurrentQueue<RIPv2EntryOrdered>();
            Parallel.For(0, id, i => {
                if (!removed.ContainsKey(i))
                {
                    try
                    {
                        q.Enqueue(new RIPv2EntryOrdered(entries[i], i));
                    }
                    catch (Exception) { }
                }
            });
            return q;

        }

    }
}
