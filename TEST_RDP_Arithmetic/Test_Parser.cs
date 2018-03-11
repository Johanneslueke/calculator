using System;
using Calculater_eXtreme;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TEST_RDP_Arithmetic
{
    [TestClass]
    public class Test_Parser
    {
        [TestMethod]
        public void Test_NumberInput0to9()
        {
            for(int i = 0; i< 9; i++)
            {
                if (i != (int)SingleNumber(i.ToString()))
                    Assert.Fail("Single digit was not parsed correctly");
            }

        }

        [TestMethod]
        public void Test_ArithmeticExpressions()
        {
            var parser = new Parser("1+2+3");
            var result = parser.EvaluateExpression();

            if(result != (1+2+3))
                Assert.Fail("1 + 2 + 3 is not " + result);

            if (parser.EvaluateExpression("2+5*3") != (2+5*3))
                Assert.Fail("2 + 5 * 3 is not " + result);

        }




        public double SingleNumber(string expression)
        {
            var parser = new Parser(expression);
            return parser.EvaluateExpression();
        }
    }
}
