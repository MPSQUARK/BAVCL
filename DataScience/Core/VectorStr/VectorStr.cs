using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataScience.Core
{
    public class VectorStr
    {

        public string[] Value;

        public VectorStr(string[] value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            VectorStr vectorStr;
            try
            {
                vectorStr = (VectorStr)obj;
            }
            catch (Exception)
            {
                return false;
            }

            if (Value.Length != vectorStr.Value.Length) { return false; }

            for (int i = 0; i < Value.Length; i++)
            {
                if (Value[i] != vectorStr.Value[i]) { return false; }
            }

            return true;
        }

        public int Max(int Threads = 2)
        {
            ConcurrentDictionary<int, int> maxlens = new ConcurrentDictionary<int, int>();

            Parallel.For(0, Value.Length, new ParallelOptions { MaxDegreeOfParallelism = Threads }, i =>
            {
                int id = Thread.CurrentThread.ManagedThreadId;

                if (maxlens.ContainsKey(id))
                {
                    if (maxlens[id] < Value[i].Length)
                    {
                        maxlens[id] = Value[i].Length;
                    }
                }
                else
                {
                    maxlens.TryAdd(id, Value[i].Length);
                }
            });

            int maxlen = 0;
            foreach (int item in maxlens.Values)
            {
                if (maxlen < item)
                {
                    maxlen = item;
                }
            }

            return maxlen;
        }

        public int Min(int Threads = 2)
        {
            ConcurrentDictionary<int, int> minlens = new ConcurrentDictionary<int, int>();

            Parallel.For(0, Value.Length, new ParallelOptions { MaxDegreeOfParallelism = Threads }, i =>
            {
                int id = Thread.CurrentThread.ManagedThreadId;

                if (minlens.ContainsKey(id))
                {
                    if (minlens[id] > Value[i].Length)
                    {
                        minlens[id] = Value[i].Length;
                    }
                }
                else
                {
                    minlens.TryAdd(id, Value[i].Length);
                }
            });

            int maxlen = int.MaxValue;
            foreach (int item in minlens.Values)
            {
                if (maxlen > item)
                {
                    maxlen = item;
                }
            }

            return maxlen;
        }

        //public bool Contains(string item)
        //{
        //    if (item == null) { return false; }

        //    return Value.ToHashSet().Contains(item);
        //}

        //public bool Contains2(string item)
        //{
        //    if (item == null) { return false; }

        //    return Value.Contains(item);
        //}

        //public bool Contains3(string item, int Threads = 2)
        //{

        //    if (item == null) { return false; }

        //    for (int i = 0; i < Value.Length; i++)
        //    {
        //        if (Value[i] == item) { return true; }
        //    }

        //    return false;
        //}

    }
}
