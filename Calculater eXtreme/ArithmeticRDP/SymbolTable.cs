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
        /*
         * Prefer in-class initialization if possible!
         * Reason: The Constructor is less cluttered with 
         *         obvious stuff like calling stupid 
         *         Standard Constructors
         */
        private OrderedDictionary Table = new OrderedDictionary();
        private bool NeedsConversion = false;
        public delegate object function(params object[] x);

        /*
         * I still don't know why Properties... I kind of getted it
         * because exposing private member-variables or fields how 
         * they are called in C#, which if i remember correctly
         * is caused by the implementation of the compiler which 
         * shouts out generic messages, is a bad thing because access
         * would be uncontrolled. But why would you want break 
         * encapsulation in the first place? Right now it's for me
         * a nice syntactically workaround to circumwent encapsulation
         * 
         * Please anyone who reads this and knows better please enlighten
         * me, It would be much appricated!
         * 
         */
        public bool Radian
        {
            get => NeedsConversion;
            set => NeedsConversion = value;
        }

        static double ConvertToDegreeOrRadian(bool conv ,double x)
        {
            if (!conv)
                return (x * 180) / Math.PI; // Degree
               return x;
        }

        void CheckNumberOfArgs(int min,int max,int given)
        {
            if (given < min || given > max)
                throw new ArgumentException((max-min) + " Argument expected but received " + given);
        }


        public SymbolTable()
        {
            Table.Add("define", (function)delegate (object[] x) {

                CheckNumberOfArgs(1, 256,x.Length);
                var NewSymbol = x[0] as SymbolToken;
                StringBuilder build = new StringBuilder();

                for (int i = 1; i < x.Length; i++)
                {
                    if(x[i] is NumberToken)
                        build.Append(((NumberToken)x[i]).Value);
                    if (x[i] is SymbolToken)
                        build.Append(((SymbolToken)x[i]).Value);

                    if (x[i] is PlusToken)
                        build.Append(((PlusToken)x[i]).Value);
                    if (x[i] is MinusToken)
                        build.Append(((MinusToken)x[i]).Value);
                    if (x[i] is MulToken)
                        build.Append(((MulToken)x[i]).Value);
                    if (x[i] is DivToken)
                        build.Append(((DivToken)x[i]).Value);
                }
                   

                if(NewSymbol != null)
                {
                    try
                    {
                        Console.WriteLine(build.ToString());
                        Table.Add(NewSymbol.Value, (function)delegate (object[] xx) {
                            return new Parser(build.ToString()).EvaluateExpression();
                        });
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Replacing old definition with new one");
                        Table.Remove(NewSymbol.Value);
                        Table.Add(NewSymbol.Value, (function)delegate (object[] xx) {
                            return -1.0;
                        });
                    }
                }
                   
                   
                return x[0];
            });

            Table.Add("Two", (function)delegate (object[] x)
            {
                CheckNumberOfArgs(2, 2, x.Length);
                return (double)x[0] + (double)x[1];
            });
            Table.Add("sqrt", (function)delegate ( object[]  x)
            {
                CheckNumberOfArgs(1, 1, x.Length);
                return Math.Sqrt((double)x[0]);
            });

            Table.Add("sin", (function)delegate ( object[] x)
            {
                CheckNumberOfArgs(1, 1, x.Length);
                return ConvertToDegreeOrRadian(Radian,Math.Sin((double)x[0]));
            });

            Table.Add("tan", (function) delegate (object[] x) 
            { 
                CheckNumberOfArgs(1, 1, x.Length);
                return ConvertToDegreeOrRadian(Radian, Math.Tan((double)x[0]));
            });

            Table.Add("cos", (function)delegate ( object[]  x)
            {
              CheckNumberOfArgs(1, 1, x.Length);
              return ConvertToDegreeOrRadian(Radian, Math.Cos((double) x[0]));
            });
            Table.Add("sinh", (function)delegate ( object[]  x)
            {
              CheckNumberOfArgs(1, 1, x.Length);
              return ConvertToDegreeOrRadian(Radian, Math.Sinh((double) x[0]));
            });
            Table.Add("cosh", (function)delegate ( object[]  x)
            {
              CheckNumberOfArgs(1, 1, x.Length);
              return ConvertToDegreeOrRadian(Radian, Math.Cosh((double) x[0]));
            });
            Table.Add("tanh", (function)delegate ( object[]  x)
            {
              CheckNumberOfArgs(1, 1, x.Length);
              return ConvertToDegreeOrRadian(Radian, Math.Tanh((double) x[0]));
            });
            Table.Add("ln", (function)delegate ( object[]  x)
            {
                CheckNumberOfArgs(1, 1, x.Length);
                return Math.Log((double)x[0]);
            });
            Table.Add("log", (function)delegate ( object[]  x)
            {
                CheckNumberOfArgs(1, 1, x.Length);
                return Math.Log10((double)x[0]);
            });
            Table.Add("pi", (function)delegate ( object[]  x)
            {
                CheckNumberOfArgs(0, 0, x.Length);
                return Math.PI;
            });
            Table.Add("tau", (function)delegate ( object[]  x)
            {
                CheckNumberOfArgs(0, 0, x.Length);
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
