using System;
using System.Collections.Generic;
using Calculater_eXtreme;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TEST_RDP_Arithmetic
{
    [TestClass]
    public class Test_Parser
    {
        List<KeyValuePair<string, double>> Expressions = new List<KeyValuePair<string, double>> {
            new KeyValuePair<string, double>("1+1",2),
            new KeyValuePair<string, double>("1-1",0),
            new KeyValuePair<string, double>("1*1",1),
            new KeyValuePair<string, double>("1/1",1),
            new KeyValuePair<string, double>("1^1",1),
            new KeyValuePair<string, double>("1%1",0),
            new KeyValuePair<string, double>("1+2+3",6),
             new KeyValuePair<string, double>("3^2",9),

            new KeyValuePair<string, double>("pi",Math.PI),

            new KeyValuePair<string, double>("2*pi",Math.PI*2),
            new KeyValuePair<string, double>("pi*2",Math.PI*2),

            new KeyValuePair<string, double>("tau",Math.PI*2),
            new KeyValuePair<string, double>("tau/2",Math.PI),

             new KeyValuePair<string, double>("sin(1)",Math.Sin(1)*(180/Math.PI)),


            new KeyValuePair<string, double>("sin(pi/2)",Math.Sin(Math.PI/2)*(180/Math.PI))
        };

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
            foreach(var test in Expressions)
            {
                Console.WriteLine("Test: " + test.Key);
                var parser = new Parser(test.Key);
                var result = parser.EvaluateExpression();

                if (result != test.Value)
                    Assert.Fail("Expression [" + test.Key + "] expected result ("+test.Value+") but received -> " + result);
               
            }

        }




        public double SingleNumber(string expression)
        {
            var parser = new Parser(expression);
            return parser.EvaluateExpression();
        }
    }
}
