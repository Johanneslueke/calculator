using System;
using System.Collections.Generic;
using Calculater_eXtreme;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TEST_RDP_Arithmetic
{
    [TestClass]
    public class Test_Tokenizer
    {
        [TestMethod]
        public void Test_ArithmeticOperator()
        {
            KeyValuePair<string, List<Token>> sampleExpressionAndExpectedResult = TestExpressions()[1];

            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.Scan(sampleExpressionAndExpectedResult.Key);

            var result = new List<Token>(tokens);
            var expected = sampleExpressionAndExpectedResult.Value;

            CompareTokens(result, expected);
        }

        private void CompareTokens(List<Token> expected, List<Token> parsed)
        {
            if (expected.Count != parsed.Count)
                Assert.Fail("expected and parsed token count different");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.IsTrue(parsed[i].GetType() == expected[i].GetType());
                if (expected[i] is NumberToken)
                {
                    var ct = (expected[i] as NumberToken).Value;
                    var pt = ((NumberToken)parsed[i]).Value;

                    Assert.AreEqual(ct, pt);
                }
            }
        }


        public static List<KeyValuePair<string, List<Token>>> TestExpressions()
        {
            return new List<KeyValuePair<string, List<Token>>>
            {
                    new KeyValuePair<string, List<Token>>("1+2-3*4/5", new List<Token>
                    {
                        new NumberToken(1),
                        new PlusToken(),
                        new NumberToken(2),
                        new MinusToken(),
                        new NumberToken(3),
                        new MulToken(),
                        new NumberToken(4),
                        new DivToken(),
                        new NumberToken(5)
                    }),
                    new KeyValuePair<string, List<Token>>("(1+2)", new List<Token>
                    {
                        new OpenParenthesisToken(),
                        new NumberToken(1),
                        new PlusToken(),
                        new NumberToken(2),
                        new ClosedParenthesisToken()
                    }),
                    new KeyValuePair<string, List<Token>>("1+2-3", new List<Token>
                    {
                        new NumberToken(1),
                        new PlusToken(),
                        new NumberToken(2),
                        new MinusToken(),
                        new NumberToken(3)
                    }),
                    new KeyValuePair<string, List<Token>>("1.2", new List<Token>
                    {
                        new NumberToken(1.2),
                    }),
                    new KeyValuePair<string, List<Token>>("0.", new List<Token>
                    {
                        new NumberToken(0),
                    }),
                    new KeyValuePair<string, List<Token>>(".2", new List<Token>
                    {
                        new NumberToken(0.2),
                    }),
                    new KeyValuePair<string, List<Token>>("4.", new List<Token>
                    {
                        new NumberToken(4),
                    }),

                    new KeyValuePair<string, List<Token>>(Math.PI.ToString(), new List<Token>
                    {
                        new NumberToken(Math.PI),
                    }),
                    new KeyValuePair<string, List<Token>>("sin(1)", new List<Token>
                    {
                        new SymbolToken("sin"),
                        new OpenParenthesisToken(),
                        new NumberToken(1),
                        new ClosedParenthesisToken()
                    }),
            };
        }
    }
}
