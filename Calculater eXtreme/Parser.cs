using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly string expression;
        private StringBuilder AST;
        private readonly TokenReader TokenStream;
        private readonly SymbolTable SymTable;
        private RuleTable Rule;
       
        public String ast
        {
            get
            {
                return AST.ToString();
            }
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
            SymTable = new SymbolTable();
            Rule = new RuleTable();
            AST = new StringBuilder();
            createAST();


            Rule.Append("Expression", (RuleTable.fundamental)delegate (object x)
             { 
                 bool isNegative = (bool) Rule["Sign"](x);
                 if (isNegative)
                     TokenStream.GetNext();
                 double valueOfExpression = (double) Rule["Term"](x);
                 if (isNegative)//Set us the proper signess of our number we found
                     valueOfExpression = -valueOfExpression;

                 //As long our Arithmetic Sum is not evaluated completly because we found another sum repeat
                 while (NextIsSummation())
                 {
                     Token Operator = GetTermOperand();
                     double ValueOfTerm = (double)Rule["Term"](x);

                     if (Operator is PlusToken)
                         valueOfExpression += ValueOfTerm;
                     else
                         valueOfExpression -= ValueOfTerm;
                 }
                 return valueOfExpression;
             });

            Rule.Append("Sign", (RuleTable.fundamental)delegate (object x)
            {
                return NextIsMinus();
            });

            Rule.Append("Term", (RuleTable.fundamental)delegate (object x)
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

            Rule.Append("Factor", (RuleTable.fundamental)delegate (object x)
            {
                if (NextIsDigit())
                    return Rule["RealNumber"](x);

                if (!NextIsOpeningBracket() && NextIsSymbol())
                    return Rule["Symbol"](x); 

                TokenStream.GetNext();
                double val = (double)Rule["Expression"](x);

                CheckCloseBracket();
                TokenStream.GetNext(); // Consume Evaluated Token
              
                return val;
            });

            Rule.Append("RealNumber", (RuleTable.fundamental)delegate (object x)
            {
                var next = TokenStream.GetNext();

                var nr = next as NumberToken;
                if (nr == null)
                    throw new Exception("Expecting Real number but got " + next);

                return nr.Value;
            });

            Rule.Append("Symbol", (RuleTable.fundamental)delegate (object x)
            {
                SymbolToken Symbol = TokenStream.GetNext() as SymbolToken;

                double val = 0;

                if (NextIsParamterExpr())
                {
                    CheckOpenBracket();
                    val = (double)Rule["Expression"](x); // Consumes ")"
                }
                else
                    return SymTable[Symbol.Value](0);

                return SymTable[Symbol.Value](val); ;
            });









        }

        public double EVAL()
        {
            return (double) Rule["Expression"](null);
        }

        //Evals all Sums
        public double EvalExpression()
        {
            //AST.AppendLine("{\n\t'Eval_Expression': ");
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

                //AST.Append(",\t\t'Action':  \n");

                if (Operator is PlusToken)
                {
                    valueOfExpression += ValueOfTerm;
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //    "{{\n 'type': 'plus',\n" +
                    //    " 'result': {0} }}\n" +
                    //    "}}", valueOfExpression);
                }
                    
                else
                {
                    valueOfExpression -= ValueOfTerm;
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //   "{{\n 'type': 'minus',\n" +
                    //   " 'result': {0} }}\n" +
                    //   "}}", valueOfExpression);
                }
       

            }

            //AST.AppendLine("} ");
            return valueOfExpression;
        }
       
        //Evals all Terms found in Sums
        private double EvalTerm()
        {
            //AST.Append("{\n'Evaluate_Term': ");
            //AST.Append("{\n");
            double totalVal = EvalFactor();
            //AST.Append("\n");

            while (NextIsNotSummation())
            {
                Token Operator = GetFactorOperator();
                double nextFactor = EvalFactor();

                //AST.Append(",\t\t'Action':  \n");

                if (Operator is DivToken)
                {
                    totalVal /= nextFactor;
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //    "{{\n'type': 'div',\n" +
                    //    "     'result': {0} }}\n" +
                    //    "}}", totalVal);
                }
                   
                else if (Operator is MulToken)
                {
                    totalVal *= nextFactor;
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //     "{{\n 'type': 'mul',\n" +
                    //     " 'result': {0} }}\n" +
                    //     "}}", totalVal);
                }
                    
                else if (Operator is ModToken)
                {
                    totalVal %= nextFactor;
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //     "{{\n 'type': 'mod',\n" +
                    //     " 'result': {0} }}\n" +
                    //     "}}", totalVal);
                }
                else
                {
                    totalVal = Math.Pow(totalVal, nextFactor);
                    //AST.AppendFormat(" {{\n'Operator': " +
                    //    "{{\n 'type': 'pow',\n" +
                    //    " 'result': {0} }}\n" +
                    //    "}}", totalVal);
                }
              
                //AST.Append("\n");

            }
            //AST.AppendLine("} ");
            return totalVal;
        }
        
        //Evals all Factor found in Terms
        private double EvalFactor()
        {
            //AST.Append("'EvaluateFactor':");
            if (NextIsDigit())
            {
                double res = GetNumber();
                //AST.AppendFormat(" {0},", res);
                return res;
            }
               
            if (!NextIsOpeningBracket() && NextIsSymbol())
            {
                double res = EvalSymbol();
                //AST.AppendFormat(" {0},", res);
                return res;
            }

            TokenStream.GetNext();
            //AST.Append("\n");
            double val = EvalExpression();

            CheckCloseBracket();
            TokenStream.GetNext();
            //AST.AppendFormat(" -> {0}\n", val);
            //AST.AppendLine("} ");
            return val;
        }

        private double EvalSymbol()
        {
            //AST.Append("{\n'Evaluate_Letters': ");  SymbolTable[((SymbolToken)).Value]()
            SymbolToken Symbol = TokenStream.GetNext() as SymbolToken;
           // AST.Append("\n");
            double val = 0;

            if (NextIsParamterExpr())
            {
                CheckOpenBracket();
                //AST.Append("\n");
                val = EvalExpression(); // Consumes ")"
            } 
            else
            {
                var res_ = SymTable[Symbol.Value](0);
                //AST.AppendFormat(" {0}\n", res_);
                return res_;
            }

            var res = SymTable[Symbol.Value](val);
            //AST.AppendFormat("  {0} \n", res);
            //AST.AppendLine("} ");
            return res;
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
