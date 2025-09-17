using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace McpFramework.McpTypes
{
    /// <summary>
    /// Base collection class for MCP-typed values in the Types namespace
    /// </summary>
    /// <typeparam name="T">The MCP-typed value type</typeparam>
    public class McpCollection<T> : IEnumerable<T> where T : class
    {
        private readonly List<T> _items;

        protected McpCollection()
        {
            _items = new List<T>();
        }

        public McpCollection(IEnumerable<T> items)
        {
            _items = new List<T>(items ?? Enumerable.Empty<T>());
        }


        public int Count => _items.Count;

        public void Add(T item)
        {
            if (item != null)
            {
                _items.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                _items.AddRange(items.Where(item => item != null));
            }
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> AsEnumerable()
        {
            return _items.AsEnumerable();
        }

        public bool IsEmpty => _items.Count == 0;
    }
}
