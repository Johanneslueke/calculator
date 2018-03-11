using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;


namespace Calculater_eXtreme
{
    //Recursive Descent Parser for Arithmetic Expressions:
    //
    // Expression := [ "-" ] Term { ("+" | "-") Term }
    // Term       := Factor> { ( "*" | "/" | "^" ) Factor }
    // Factor     := RealNumber | "(" Expression ")"
    // RealNumber := Digit{Digit} | [Digit] "," {Digit}
    // Symbol     := Letter | Letter
    // Digit      := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 
    // Letter     := "A" | "B" | "C" | "D" | "E" | "F" | "G" | "H" | "I" | "J" | "K" |
    //               "L" | "M" | "N"| "O"  | "P" | "Q" | "R" | "S" | "T" | "U"| "V" |
    //               "W" | "X" | "Y" | "Z" |
    //               "a" | "b" | "c" | "d" | "e" | "f" | "g"| "h" | "i" | "j" | "k" |
    //               "l" | "l" | "n"| "o"  | "p" | "q" | "r" | "s" | "t" | "u"| "v" |
    //               "w" | "x" | "y" | "z" 


    public class Parser
    {
        private string expression;
        private StringBuilder AST  = new StringBuilder();
        private TokenReader TokenStream;
        private readonly SymbolTable SymTable = new SymbolTable();
        private readonly RuleTable Rule = new RuleTable();
       
        public String ast
        {
            get
            {
                return AST.ToString();
            }
        }

        public OrderedDictionary GrammarRules {
            get => Rule.RawTable;
        }

        private void createAST()
        {
            List<Token> flattenAST = (List<Token>)TokenStream.Stream;
            AST.Append("{\n");
            for(int i = 0; i < flattenAST.Count; i++)
            {
                if (flattenAST[i] is OperatorToken)
                {
                    AST.Append("\"Operator\": ");
                    if (flattenAST[i] is MinusToken) { AST.Append("minus"); }
                    if (flattenAST[i] is PlusToken) { AST.Append("plus"); }
                    if (flattenAST[i] is MulToken) { AST.Append("mul"); }
                    if (flattenAST[i] is DivToken) { AST.Append("div"); }
                    if (flattenAST[i] is ModToken) { AST.Append("mod"); }
                    if (flattenAST[i] is PowerToken) { AST.Append("pow"); }
                }

                if (flattenAST[i] is NumberToken)
                {
                    AST.AppendFormat("\"Operand\": {0}", ((NumberToken)flattenAST[i]).Value);
                }

                if (flattenAST[i] is ParenthesisToken)
                {
                    if (flattenAST[i] is OpenParenthesisToken)
                    {
                        AST.Append("\"Expression\": ");
                        AST.Append("{\n");
                        continue;
                    }
                    if (flattenAST[i] is ClosedParenthesisToken)
                        AST.Append("}\n");
                }
                if (flattenAST[i] is SymbolToken)
                {
                    AST.AppendFormat(" \"Symbol\": \"{0}\"", ((SymbolToken)flattenAST[i]).Value);

                }

                if (i + 1 != flattenAST.Count && !(flattenAST[i+1] is ClosedParenthesisToken))
                    AST.Append(",");
            }
            AST.Append("}\n");
        }

        public Parser(string expression)
        {
            this.expression = expression;
            TokenStream = new TokenReader(new Tokenizer().Scan(this.expression));

            Rule.Append("Expression", (RuleTable.fundamental)delegate (object[] x)
             {
                 if (NextIs(typeof(SymbolToken)))
                 {
                     return Rule["Symbol"](); ;
                 }
                 bool isNegative = NextIsMinus();
                 if (isNegative)
                     TokenStream.GetNext();

                 double valueOfExpression = (double)Rule["Term"](x);
                 if (isNegative)//Set us the proper signess of our number we found
                     valueOfExpression = -valueOfExpression;

                 //As long our Arithmetic Sum is not evaluated completly because we found another sum repeat#
                 while (NextIsSummation())
                 {
                     Token Operator = GetTermOperator();
                     double ValueOfTerm = (double)Rule["Term"](x);

                     if (Operator is PlusToken)
                         valueOfExpression += ValueOfTerm;
                     else if (Operator is MinusToken)
                         valueOfExpression -= ValueOfTerm;
                 }
                 return valueOfExpression;
             });

            Rule.Append("Term", (RuleTable.fundamental)delegate (object[] x)
            {
                double totalVal = (double)Rule["Factor"](x);

                while (NextIsNotSummation())
                {
                    Token Operator = GetFactorOperator();
                    double nextFactor = (double)Rule["Factor"](x);

                    if (Operator is DivToken)
                        totalVal /= nextFactor;
                    else if (Operator is MulToken)
                        totalVal *= nextFactor;
                    else if (Operator is ModToken)
                        totalVal %= nextFactor;
                    else
                        totalVal = Math.Pow(totalVal, nextFactor);


                }
                return totalVal;
            });

            Rule.Append("Factor", (RuleTable.fundamental)delegate (object[] x)
            {

                var isNumber = Rule["RealNumber"](x);
                if (isNumber != null)
                    return isNumber;

                var isSymbol = Rule["Symbol"](x);
                if (isSymbol != null)
                    return isSymbol;

                TokenStream.GetNext();

                double val = (double)Rule["Expression"](x);
                CheckCloseBracket();
                TokenStream.GetNext(); // Consume Evaluated Token

                return val;
            });

            Rule.Append("RealNumber", (RuleTable.fundamental)delegate (object[] x)
            {
                if (NextIsDigit())
                {
                    var next = TokenStream.GetNext();

                    var nr = next as NumberToken;
                    if (nr == null)
                        throw new Exception("Expecting Real number but got " + next);

                    return nr.Value;
                }

                return null;
                   
            });

            Rule.Append("Paramter", (RuleTable.fundamental) delegate (object[] x)
            {
                Rule.CheckNumberOfArgs(0, 256,x.Length);

                while(!NextIsClosingBracket() || NextIs(typeof(CommaToken)))
                {
                    return Rule["Expression"](x);
                }
                return null;
            });

            Rule.Append("Symbol", (RuleTable.fundamental)delegate (object[] x)
            {
                if (!NextIsOpeningBracket() || NextIsSymbol())
                {
                    SymbolToken Symbol = TokenStream.GetNext() as SymbolToken;

                    double val = 0;


                    if (NextIs(typeof(SymbolToken)))
                    {
                        List<Token> args = new List<Token>();
                        while (TokenStream.TokensAvailable)
                        {
                            args.Add(TokenStream.GetNext());
                        }
                        return SymTable[Symbol.Value](args.ToArray());
                    }

                    //decides syntax between Symbol Constant and Symbol Function !!!
                    //If ParameterExpression is found because after the Symbol we 
                    //found a open Parentethis
                    if (NextIsParamterExpr() && !NextIsClosingBracket())
                    {

                        CheckOpenBracket();
                        val = (double)Rule["Paramter"](x); // Consumes ")"
                    }
                    else
                    {
                        var res = SymTable[Symbol.Value]();

                        return res;
                    }

                    return SymTable[Symbol.Value](val); ;
                }

                return null;
                    
            });

        }

        public double EvaluateExpression()
        {
            var res = Rule["Expression"]();
            if (res is double)
                return (double)res;
            return 0;
        }

        public double EvaluateExpression(string expr)
        {
             expression = expr;
            TokenStream = new TokenReader(new Tokenizer().Scan(this.expression));
            object res = Rule["Expression"]();
            if (!(res is Token))
                return (Double)res;
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
        private bool NextIsClosingBracket()
        {
            return NextIs(typeof(ClosedParenthesisToken));
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
        private bool NextIsSymbol()
        {
            if (!TokenStream.TokensAvailable)
                return false;
            return TokenStream.PeekNext() is SymbolToken;
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
                &&  ( NextIs(typeof(MulToken)) 
                   || NextIs(typeof(DivToken)) 
                   || NextIs(typeof(PowerToken)) 
                   || NextIs(typeof(ModToken)) );
        }

        private bool NextIsParamterExpr()
        {
            return TokenStream.TokensAvailable && NextIsOpeningBracket() ;
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
            if (!(NextIsClosingBracket()) || !TokenStream.TokensAvailable)
                throw new Exception(
                    "Expecting ')' in expression, instead got: "
                    + (PeekNext() != null ?
                    PeekNext().ToString() : "End of expression")
                    );
        }

        private Token GetTermOperator()
        {
            var c = TokenStream.GetNext();
            if (c is PlusToken)
                return c;
            if (c is MinusToken)
                return c;

            throw new Exception("Expected term operand '+' or '-' but found" + c);
        }

        private Token GetFactorOperator()
        {
            var c = TokenStream.GetNext();
            if (c is DivToken)
                return c;
            if (c is MulToken)
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

       


    }


}
