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
    class Tokenizer
    {
        private StringReader Stream; //emulates Stream of Characters

        //Scans an string and lists all found tokens which follow the 
        //coventinal rules known of mathematics. Might be possible
        //to actually merge both the scan and the evalutation into one
        //Function so you it might saves you from the TagDispatching 
        //mechanism. But for simpler extensibilty  it might be nice to
        //be able to split those two task!
        public IEnumerable<Token> Scan(string expression)
        {
            Stream = new StringReader(expression);

            var tokens = new List<Token>();
            while (Stream.Peek() != -1)
            {
                var c = (char)Stream.Peek();
                if (Char.IsWhiteSpace(c)) //Ignore these nasty char no one can see
                {
                    Stream.Read();
                    continue;
                }

                if (Char.IsDigit(c) || c == ',')
                {
                    tokens.Add(new NumberToken(EvaluateNumberExpr()));
                }
                else if (c == '-')
                {
                    tokens.Add(new MinusToken());
                    Stream.Read();
                }
                else if (c == '+')
                {
                    tokens.Add(new PlusToken());
                    Stream.Read();
                }
                else if (c == '*')
                {
                    tokens.Add(new MultiplyToken());
                    Stream.Read();
                }
                else if (c == '%')
                {
                    tokens.Add(new ModToken());
                    Stream.Read();
                }
                else if (c == '/')
                {
                    tokens.Add(new DivideToken());
                    Stream.Read();
                }
                else if (c == '(')
                {
                    tokens.Add(new OpenParenthesisToken());
                    Stream.Read();
                }
                else if (c == ')')
                {
                    tokens.Add(new ClosedParenthesisToken());
                    Stream.Read();
                }
                else if (c == '^')
                {
                    tokens.Add(new PowerToken());
                    Stream.Read();
                }
                else if (c >= 'a' || c <= 'Z')
                {
                    tokens.Add(new LetterToken(c));
                    Stream.Read();
                }
                else
                    throw new Exception("Unknown character in expression: " + c);
            }

            return tokens;
        }

        
        private double EvaluateNumberExpr()
        {
            var sb = new StringBuilder();
            var decimalExists = false;
            while (Char.IsDigit((char)Stream.Peek()) || ((char)Stream.Peek() == ','))
            {
                var digit = (char)Stream.Read();
                if (digit == ',')
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
