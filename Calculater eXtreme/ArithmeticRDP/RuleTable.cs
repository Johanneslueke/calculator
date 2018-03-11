using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    class RuleTable : IDictionary<String,object>
    {
        private OrderedDictionary Table  = new OrderedDictionary();
        public delegate object fundamental(params object[] condition);

        public RuleTable()
        {

        }

        public RuleTable(List<KeyValuePair<object,object>> rules)
        {
            foreach (var item in rules)
                Table.Add(item.Key, item.Value);
        }

        public void CheckNumberOfArgs(int min, int max, int given)
        {
            if (given < min || given > max)
                throw new ArgumentException((max - min) + " Argument expected but received " + given);
        }

        public OrderedDictionary RawTable
        {
            get
            {
                return Table;
            }
        }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<object> Values => throw new NotImplementedException();

        public int Count => Table.Count;

        public bool IsReadOnly => Table.IsReadOnly;

        object IDictionary<string, object>.this[string key]
        {
            get => Table[key];
            set => Table[key] = value; }

        public void Append(object Name,object Rule)
        {
            Table.Add(Name,Rule);
        }

        public bool ContainsKey(string key)
        {
            return Table.Contains(key);
        }

        public void Add(string key, object value)
        {
            this.Append(key, value);
        }

        public bool Remove(string key)
        {
            Table.Remove(key);
            return ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            value = Table[key];
            if (value != null)
                return true;
            return false;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Table.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Table.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Table.Contains(item.Key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            Table.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            Table.Remove(item.Key);
            return ContainsKey(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<string, object>>)Table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public fundamental this[String key]
        {
            get
            {
                return (fundamental)Table[key];
            }
        }
    }
}
