using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    //Essential this is the equivialent of Tag Dispatching
    //in C++. Token is an abstract class which is the base 
    //for all Token types. Each Instanciation of one of Tokens 
    //decendents represent an different kind of Token simply by
    //differentiating their different class Names. Through this
    //each kind of Token represent its own Identity within its 
    //class of Token. Only Exception is the NumberToken which 
    //represent something very concrete: A Number and a Type of Token

    public abstract class Token
    {
    }

    public class NumberToken : Token
    {
        private readonly double _value;

        public NumberToken(double value)
        {
            _value = value;
        }

        public double Value
        {
            get { return _value; }
        }
    }

    public class OperatorToken : Token
    {

    }
    public class PlusToken : OperatorToken
    {
    }

    public class MinusToken : OperatorToken
    {
    }

    public class MulToken : OperatorToken
    {
    }

    public class DivToken : OperatorToken
    {
    }

    public class ModToken : OperatorToken
    {

    }
    public class PowerToken : OperatorToken
    {

    }


    public class LetterToken : Token
    {
        private readonly char _value;

        public LetterToken(char value)
        {
            _value = value;
        }

        public char Value
        {
            get { return _value; }
        }
    }

    public class SymbolToken : Token
    {
        private readonly String SymbolName;

        public SymbolToken(String value)
        {
            SymbolName = value;
        }

        public String Value
        {
            get {
                return SymbolName;
            }
        }

    }


    public class ParenthesisToken : Token
    {

    }

    public class OpenParenthesisToken : ParenthesisToken
    {
    }

    public class ClosedParenthesisToken : ParenthesisToken
    {
    }

    
}
