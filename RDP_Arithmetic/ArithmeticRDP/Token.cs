using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    /// <summary>
    /// Token is the abstract base class for any token defined here.
    /// It does not contain any inforcable contract/method. it just
    /// represent the common root for all our problems ;)
    /// 
    /// Essential this is the equivialent of Tag Dispatching
    /// in C++. Token is an abstract class which is the base 
    /// for all Token types. Each Instanciation of one of Tokens 
    /// decendents represent an different kind of Token simply by
    /// differentiating their different class Names. Through this
    /// each kind of Token represent its own Identity within its 
    /// class of Token. Only Exception is the NumberToken which 
    /// represent something very concrete: A Number and a Type of Token
    /// </summary>
    public abstract class Token
    {
    }

    /// <summary>
    /// Any Number recognized by an tokenizer is represented through this
    /// NumberToken
    /// </summary>
    public class NumberToken : Token
    {
        private readonly double _value;

        /// <summary>
        /// Initialize the actual value of a numbertoken so that we can
        /// interface with it with the rest of our program
        /// </summary>
        /// <param name="value"> sets the value of the token</param>
        public NumberToken(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public double Value
        {
            get { return _value; }
        }
    }

    /// <summary>
    /// Similar to the Token base class this represents the base
    /// of all Operator Tokens
    /// </summary>
    public class OperatorToken : Token
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class PlusToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "+";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MinusToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "-";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MulToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "*";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DivToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "/";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ModToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "%";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PowerToken : OperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "^";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SeperatorToken : Token
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class CommaToken : SeperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => ",";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SemiColonToken : SeperatorToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => ";";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LetterToken : Token
    {
        private readonly char _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public LetterToken(char value)
        {
            _value = value;
        }

        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public char Value
        {
            get { return _value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SymbolToken : Token
    {
        private readonly String SymbolName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public SymbolToken(String value)
        {
            SymbolName = value;
        }

        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get {
                return SymbolName;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ExpressionToken : Token
    {
        private readonly String Expression;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public ExpressionToken(String value)
        {
            Expression = value;
        }

        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get
            {
                return Expression;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ParenthesisToken : Token
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class OpenParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "(";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ClosedParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => ")";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OpenCurlyParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "{";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ClosedCurlyParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OpenSquaredParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "[";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ClosedSquaredParenthesisToken : ParenthesisToken
    {
        /// <summary>
        /// Returns the value of the token
        /// </summary>
        public String Value
        {
            get => "]";
        }
    }



}
