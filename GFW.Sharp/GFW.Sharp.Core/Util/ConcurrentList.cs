using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Util
{
    public class ConcurrentList<T>
    {
        private List<T> _list = new List<T>();
        private object _sync = new object();
        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }
        public T Find(Predicate<T> predicate)
        {
            lock (_sync)
            {
                return _list.Find(predicate);
            }
        }
        public T FirstOrDefault()
        {
            lock (_sync)
            {
                return _list.FirstOrDefault();
            }
        }

        public void AddRange(T[] value)
        {
            lock (_sync)
            {
                _list.AddRange(value);
            }
        }

        public void RemoveRange(int index, int count)
        {
            lock (_sync)
            {
                _list.RemoveRange(index, count);
            }
        }

        public int PopRange(T[] value, int count)
        {
            lock (_sync)
            {
                int ret = Math.Min(Math.Min(value.Length, _list.Count), count);
                for(int i =0;i<ret;i++)
                {
                    value[i] = _list[i];
                }
                _list.RemoveRange(0, ret);
                return ret;
            }
        }

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _list.Count;
                }
            }
        }
    }
}
