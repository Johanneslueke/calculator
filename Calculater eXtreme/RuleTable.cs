using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    class RuleTable
    {
        private OrderedDictionary Table;
        public delegate object fundamental(object condition);

        public RuleTable()
        {
            Table = new OrderedDictionary();
        }

        public void Append(object Name,object Rule)
        {
            Table.Add(Name,Rule);
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
