using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber.Module
{
    public static class LispNumber
    {
        [FunctionName("*")]
        [FunctionName("mul")]
        [FunctionName("mult")]
        [FunctionName("multiply")]
        [FunctionName("prod")]
        [FunctionName("product")]
        public static ILispNode Prod(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(new LispMissing(), (r, x) => (double) r * (double) x);

                    var result = functor.MergeAsNumber(arguments, callStack, 2, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch
                {
                    throw new Exception("PROD requires all arguments to be numbers");
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("prod threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(prod {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("/")]
        [FunctionName("div")]
        public static ILispNode Div(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(
                        new LispMissing(),
                        (r, x) =>
                        {
                            if ((double) x == 0.0d)
                            {
                                throw new Exception("DIV by 0");
                            }
                            return (double) r / (double) x;
                        });

                    var result = functor.MergeAsNumber(arguments, callStack, 2, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch (Exception ex)
                {
                    throw new LispException(null, "DIV throws exception!", ex);
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("div threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(div {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("~")]
        [FunctionName("-")]
        [FunctionName("negate")]
        public static ILispNode Negate(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(new LispMissing(), (r, x) => -1 * (double) x);

                    var result = functor.MergeAsNumber(arguments, callStack, 1, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch (Exception ex)
                {
                    throw new LispException(null, "NEGATE throws exception!", ex);
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("negate threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(negate {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("number")]
        [FunctionName("double")]
        public static ILispNode Double(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var xEval = arguments[0].Eval(callStack, true);
                    if (xEval is LispAtom)
                    {
                        double result;
                        if (double.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
                        {
                            return new LispAtom(result);
                        }
                    }

                    throw new Exception();
                }
                catch
                {
                    return new LispNil();
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("double threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(double {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("number?")]
        [FunctionName("number-p")]
        public static ILispNode NumberP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var xEval = arguments[0].Eval(callStack, true);
                    if (xEval is LispAtom)
                    {
                        double result;
                        if (double.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
                        {
                            return new LispAtom(true);
                        }
                    }

                    throw new Exception();
                }
                catch
                {
                    return new LispAtom(false);
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("number_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(number_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("int")]
        public static ILispNode Int(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var xEval = arguments[0].Eval(callStack, true);
                    if (xEval is LispAtom)
                    {
                        int result;
                        if (int.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
                        {
                            return new LispAtom(result);
                        }
                    }

                    throw new Exception();
                }
                catch
                {
                    return new LispNil();
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("int threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(int {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("int?")]
        [FunctionName("int-p")]
        public static ILispNode IntP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var xEval = arguments[0].Eval(callStack, true);
                    if (xEval is LispAtom)
                    {
                        int result;
                        if (int.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
                        {
                            return new LispAtom(true);
                        }
                    }

                    throw new Exception();
                }
                catch
                {
                    return new LispAtom(false);
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("int_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(int_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }
    }
}