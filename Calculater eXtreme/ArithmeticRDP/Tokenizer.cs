using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Calculater_eXtreme
{
    //Recursive Descent Parser for Arithmetic Expressions:
    //
    // Expression := [ "-" ] Term { ("+" | "-") Term }
    // Term       := Factor> { ( "*" | "/" | "^" ) Factor }
    // Factor     := RealNumber | "(" Expression ")"
    // RealNumber := Digit{Digit} | [Digit] "," {Digit}
    // Symbol     := Letter | Letter,Digit | "(" Expression ")"
    // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 
    // Letter     := "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" | "J" | "K" |
    //               "L" | "M" | "N"| "O"  | "P" | "Q" | "R" | "S" | "T" | "U"| "V" |
    //               "W" | "X" | "Y" | "Z" |
    //               "a" | "b" | "c" | "d" | "e" | "f" | "g"| "h" | "i" | "j" | "k" |
    //               "l" | "l" | "n"| "o"  | "p" | "q" | "r" | "s" | "t" | "u"| "v" |
    //               "w" | "x" | "y" | "z"  
    class Tokenizer : IDisposable
    {
        private StringReader Stream; //emulates Stream of Characters
        private RuleTable Rule = new RuleTable();
        private List<Token> tokens = new List<Token>();

        public void Dispose() => Stream.Dispose();


        public Tokenizer()
        {            
            Rule.Append("IgnoreWhiteSpaces", (RuleTable.fundamental)delegate (object[] c) {

                Rule.CheckNumberOfArgs(1, 1, c.Length);
                if (Char.IsWhiteSpace((char)c[0]))
                {
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseNumber", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char) x[0];

                if (Char.IsDigit(c) || c == ',')
                {
                    tokens.Add(new NumberToken(EvaluateNumberExpr()));
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorMinus", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '-')
                {
                    tokens.Add(new MinusToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorPlus", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '+')
                {
                    tokens.Add(new PlusToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorMul", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '*')
                {
                    tokens.Add(new MulToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorDiv", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '/')
                {
                    tokens.Add(new DivToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorMod", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '%')
                {
                    tokens.Add(new ModToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseOperatorPow", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c == '^')
                {
                    tokens.Add(new PowerToken());
                    Stream.Read();
                    return true;
                }

                return false;
            });

            Rule.Append("RecogniseParenthesis", (RuleTable.fundamental)delegate (object[] x)
            {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];
                if (c == '(')
                {
                    tokens.Add(new OpenParenthesisToken());
                    Stream.Read();
                    return true;
                }
                else if (c == ')')
                {
                    tokens.Add(new ClosedParenthesisToken());
                    Stream.Read();
                    return true;
                }

                if (c == '{')
                {
                    tokens.Add(new OpenCurlyParenthesisToken());
                    Stream.Read();
                    return true;
                }
                else if (c == '}')
                {
                    tokens.Add(new ClosedCurlyParenthesisToken());
                    Stream.Read();
                    return true;
                }

                if (c == '[')
                {
                    tokens.Add(new OpenSquaredParenthesisToken());
                    Stream.Read();
                    return true;
                }
                else if (c == ']')
                {
                    tokens.Add(new ClosedSquaredParenthesisToken());
                    Stream.Read();
                    return true;
                }

                return false;

            });

            Rule.Append("RecogniseSymbol", (RuleTable.fundamental)delegate (object[] x) {
                Rule.CheckNumberOfArgs(1, 1, x.Length);
                char c = (char)x[0];

                if (c >= 'a' || c <= 'Z')
                {
                    //tokens.Add(new LetterToken(c));
                    StringBuilder sym = new StringBuilder();
                    while (Char.IsLetter((char)Stream.Peek()))
                    {
                        c = (char)Stream.Peek();
                        sym.Append(c);
                        Stream.Read();
                    }
                    tokens.Add(new SymbolToken(sym.ToString()));
                    //Stream.Read();

                    return true;
                }

                return false;

            });

           

        }

        //Scans an string and lists all found tokens which follow the 
        //coventinal rules known of mathematics. Might be possible
        //to actually merge both the scan and the evalutation into one
        //Function so you it might saves you from the TagDispatching 
        //mechanism. But for simpler extensibilty  it might be nice to
        //be able to split those two task!
        public IEnumerable<Token> Scan(string expression)
        {
            Stream = new StringReader(expression);
            
            while (Stream.Peek() != -1)
            {
                var c = (char)Stream.Peek();
                if ((bool) Rule["IgnoreWhiteSpaces"](c)) 
                    continue;

                if(!((bool) Rule["RecogniseNumber"](c)
                 || (bool) Rule["RecogniseOperatorMinus"](c)
                 || (bool) Rule["RecogniseOperatorPlus"](c)
                 || (bool) Rule["RecogniseOperatorMul"](c)
                 || (bool) Rule["RecogniseOperatorDiv"](c)
                 || (bool) Rule["RecogniseOperatorMod"](c)
                 || (bool) Rule["RecogniseOperatorPow"](c)
                 || (bool) Rule["RecogniseParenthesis"](c)
                 || (bool) Rule["RecogniseSymbol"](c)))
                    throw new Exception("Unknown character in expression: " + c);
            }

            return tokens;
        }

        
        private double EvaluateNumberExpr()
        {
            var sb = new StringBuilder();
            var decimalExists = false;
            while (Char.IsDigit((char)Stream.Peek()) || ((char)Stream.Peek() == '.'))
            {
                var digit = (char)Stream.Read();
                if (digit == '.')
                {
                    if (decimalExists) throw new Exception("Multiple comma in decimal number");
                    decimalExists = true;
                }
                sb.Append(digit);
            }

            double res;
            if (!double.TryParse(sb.ToString(), out res))
                throw new Exception("Could not parse number: " + sb);

            return res;
        }
    }
}
