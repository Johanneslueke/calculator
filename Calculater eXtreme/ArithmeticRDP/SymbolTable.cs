using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    class SymbolTable
    {
        private OrderedDictionary Table;
        private bool NeedsConversion;
        public delegate double function(double x);
        public bool Radian
        {
            get => NeedsConversion;
            set => NeedsConversion = value;
        }


        public SymbolTable()
        {
            Table = new OrderedDictionary();
            Table.Add("sqrt", (function)delegate (double x)
            {
                return Math.Sqrt(x);
            });
            Table.Add("sin", (function)delegate (double x)
            {
                x = Math.Sin(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("tan", (function)delegate (double x)
            {
                x = Math.Tan(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("cos", (function)delegate (double x)
            {
                x = Math.Cos(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("sinh", (function)delegate (double x)
            {
                x = Math.Sinh(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("cosh", (function)delegate (double x)
            {
                x = Math.Cosh(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("tanh", (function)delegate (double x)
            {
                x = Math.Tanh(x); ;
                if (!Radian)
                    x = (x * 180) / Math.PI; // Degree
                else
                    return x;
                return x;
            });
            Table.Add("ln", (function)delegate (double x)
            {
                return Math.Log(x);
            });
            Table.Add("log", (function)delegate (double x)
            {
                return Math.Log10(x);
            });
            Table.Add("pi", (function)delegate (double x)
            {
                return Math.PI;
            });
            Table.Add("tau", (function)delegate (double x)
            {
                return Math.PI*2;
            });
        }

        public function this[String key]
        {
            get
            {
                return (function)Table[key];
            }
        }
    }
}
