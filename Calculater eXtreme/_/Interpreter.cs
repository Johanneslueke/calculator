using System;

namespace BrightSword.LightSaber
{
    public class Interpreter
    {
        private readonly CallStack _callStack = new CallStack();

        public Interpreter Initialize(params string [ ] rgExpressions)
        {
            if (rgExpressions == null)
            {
                return this;
            }

            foreach (var t in rgExpressions)
            {
                t.Parse().Eval(_callStack);
            }

            return this;
        }

        public object Execute(string strExpressionFormat, params object [ ] args)
        {
            var strExpression = String.Format(strExpressionFormat, args);
            return strExpression.Parse().Eval(_callStack).Value;
        }
    }
}