using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    //Recursive Descent Parser for Arithmetic Expressions:
    //
    // Expression := [ "-" ] Term { ("+" | "-") Term }
    // Term       := Factor> { ( "*" | "/" | "^" ) Factor }
    // Factor     := RealNumber | "(" Expression ")"
    // RealNumber := Digit{Digit} | [Digit] "," {Digit}
    // Symbol     := Letter | Letter,Digit
    // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 
    // Letter     := "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" | "J" | "K" |
    //               "L" | "M" | "N"| "O"  | "P" | "Q" | "R" | "S" | "T" | "U"| "V" |
    //               "W" | "X" | "Y" | "Z" |
    //               "a" | "b" | "c" | "d" | "e" | "f" | "g"| "h" | "i" | "j" | "k" |
    //               "l" | "l" | "n"| "o"  | "p" | "q" | "r" | "s" | "t" | "u"| "v" |
    //               "w" | "x" | "y" | "z" 


    public class Parser
    {
        private readonly string expression;
        private readonly TokenReader TokenStream;


        public Parser(string expression)
        {
            this.expression = expression;
            TokenStream = new TokenReader(new Tokenizer().Scan(this.expression));
        }

        public double EvalExpression()
        {
            bool isNegative = NextIsMinus();
            if (isNegative)
                TokenStream.GetNext();
            double valueOfExpression = EvalTerm();

            if (isNegative)//Set us the proper signess of our number we found
                valueOfExpression = -valueOfExpression;

            //As long our Arithmetic Sum is not evaluated completly because we found another sum repeat
            while (NextIsSummation()) 
            {
                Token Operator = GetTermOperand();
                double ValueOfTerm = EvalTerm();

                if (Operator is PlusToken)
                    valueOfExpression += ValueOfTerm;
                else
                    valueOfExpression -= ValueOfTerm;
            }
            return valueOfExpression;
        }
       
        private double EvalTerm()
        {
            double totalVal = EvalFactor();

            while (NextIsNotSummation())
            {
                Token Operator = GetFactorOperand();
                double nextFactor = EvalFactor();

                if (Operator is DivideToken)
                    totalVal /= nextFactor;
                else if (Operator is MultiplyToken)
                    totalVal *= nextFactor;
                else if (Operator is ModToken)
                    totalVal %= nextFactor;
                else
                    totalVal = Math.Pow(totalVal, nextFactor);
            }

            return totalVal;
        }
        
        private double EvalFactor()
        {
            if (NextIsDigit())
                return GetNumber();
            if (!NextIsOpeningBracket())
                return EvalLetter();

            TokenStream.GetNext();

            double val = EvalExpression();

            CheckCloseBracket();
            TokenStream.GetNext();
            return val;
        }

        private double EvalLetter()
        {
            SymbolToken Symbol = GetSymbol() as SymbolToken;
            double val = 0;

            if (NextIsParamterExpr())
            {
                CheckOpenBracket();
                val = EvalExpression(); // Consumes ")"
            } 
            else
                return EvalSymbol(Symbol, null);;
            return EvalSymbol(Symbol,val);
        }

        private double EvalSymbol(Token Sym, double? ArgValue)
        {
            var Symbol = (Sym as SymbolToken).Value;
            if (ArgValue == null)
            {
                switch (Symbol)
                {
                    case "tau":
                        return Math.PI*2;
                    case "pi":
                        return Math.PI;
                }
            }
            double val = (ArgValue.Value);
            switch (Symbol)
            {
                case "sqrt":
                    return Math.Sqrt(val);
                case "sin":
                    return Math.Sin(val);
                case "tan":
                    return Math.Tan(val);
                case "cos":
                    return Math.Cos(val);
                case "sinh":
                    return Math.Sinh(val);
                case "cosh":
                    return Math.Cosh(val);
                case "tanh":
                    return Math.Tanh(val);
                case "ln":
                    return Math.Log(val);
                case "log":
                    return Math.Log10(val);

            }

            return 0;
        }

        private bool NextIsMinus()
        {
            return TokenStream.TokensAvailable && TokenStream.IsNextOfType(typeof(MinusToken));
        }

        private bool NextIsOpeningBracket()
        {
            return NextIs(typeof(OpenParenthesisToken));
        }

        private bool NextIsDigit()
        {
            if (!TokenStream.TokensAvailable)
                return false;
            return TokenStream.PeekNext() is NumberToken;
        }

        private bool NextIsLetter()
        {
            if (!TokenStream.TokensAvailable)
                return false;
            return TokenStream.PeekNext() is LetterToken;
        }

        private bool NextIs(Type type)
        {
            return TokenStream.TokensAvailable && TokenStream.IsNextOfType(type);
        }


        //Because in math we calc division and multiplication before any summation
        //we need to distinguish between those two classes of operation so we might
        //be able to suspend a summation so multiplication or division can be done
        //first
        private bool NextIsSummation()
        {
            return TokenStream.TokensAvailable && (NextIs(typeof(MinusToken)) || NextIs(typeof(PlusToken)));
        }

        private bool NextIsNotSummation()
        {
            return TokenStream.TokensAvailable 
                &&  ( NextIs(typeof(MultiplyToken)) 
                   || NextIs(typeof(DivideToken)) 
                   || NextIs(typeof(PowerToken)) 
                   || NextIs(typeof(ModToken)) );
        }

        private bool NextIsParamterExpr()
        {
            return TokenStream.TokensAvailable && !NextIsNotSummation() ;
        }

        private void CheckOpenBracket()
        {
            if (!NextIsOpeningBracket() || !TokenStream.TokensAvailable)
                throw new Exception(
                    "Expecting a number or '(' in expression, instead got : "
                    + ((PeekNext() != null) ?
                    PeekNext().ToString() : "End of expression")
                    );
        }

        private void CheckCloseBracket()
        {
            if (!(NextIs(typeof(ClosedParenthesisToken))))
                throw new Exception(
                    "Expecting ')' in expression, instead got: "
                    + (PeekNext() != null ?
                    PeekNext().ToString() : "End of expression")
                    );
        }

        private Token GetTermOperand()
        {
            var c = TokenStream.GetNext();
            if (c is PlusToken)
                return c;
            if (c is MinusToken)
                return c;

            throw new Exception("Expected term operand '+' or '-' but found" + c);
        }

        private Token GetFactorOperand()
        {
            var c = TokenStream.GetNext();
            if (c is DivideToken)
                return c;
            if (c is MultiplyToken)
                return c;
            if (c is PowerToken)
                return c;
            if (c is ModToken)
                return c;

            throw new Exception("Expected factor operand '/' or '*' or '^' or '%' but found" + c);
        }

        private Token PeekNext()
        {
            return TokenStream.TokensAvailable ? TokenStream.PeekNext() : null;
        }

        private double GetNumber()
        {
            var next = TokenStream.GetNext();

            var nr = next as NumberToken;
            if (nr == null)
                throw new Exception("Expecting Real number but got " + next);

            return nr.Value;
        }

        private Token GetSymbol()
        {
            List<LetterToken> SymbolChar = new List<LetterToken>();
            while (NextIsLetter())
            {
                SymbolChar.Add(TokenStream.GetNext() as LetterToken);
            }

            return new SymbolToken(SymbolChar);
        }


    }
}
