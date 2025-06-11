using System.Collections.Generic;

namespace Util
{
    public class OrderedSet<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new();
        private readonly HashSet<T> _set = new();

        public bool Add(T item)
        {
            if (_set.Add(item))
            {
                _list.Add(item);
                return true;
            }
            return false;
        }

        public int Count => _list.Count;
        
        public T this[int index] => _list[index];
        
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}