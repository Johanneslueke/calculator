using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber.Module
{
    public static class LispBoolean
    {
        [FunctionName("bool")]
        public static ILispNode Bool(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var xEval = arguments[0].Eval(callStack, true);
                    if (xEval is LispMissing)
                    {
                        return xEval;
                    }

                    if (xEval is LispAtom)
                    {
                        bool result;
                        if (bool.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
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
                Console.WriteLine("bool threw Exception" + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(bool {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("bool?")]
        [FunctionName("bool-p")]
        public static ILispNode BoolP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
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
                        bool result;
                        if (bool.TryParse((xEval as LispAtom).ValueAsNumber.ToString(), out result))
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
                Console.WriteLine("bool_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(bool_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("false")]
        [FunctionName("f")]
        public static ILispNode False(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                return new LispAtom(false);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("false threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(false {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("true")]
        [FunctionName("t")]
        public static ILispNode True(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                return new LispAtom(true);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("true threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(true {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("&&")]
        [FunctionName("and")]
        [FunctionName("all")]
        public static ILispNode And(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(new LispMissing(), (r, x) => (bool) r && (bool) x);

                    var result = functor.MergeAsBoolean(arguments, callStack, 2, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch
                {
                    throw new Exception("AND requires its arguments to all be Boolean");
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("and threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(and {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("||")]
        [FunctionName("or")]
        [FunctionName("any")]
        public static ILispNode Or(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(new LispMissing(), (r, x) => (bool) r || (bool) x);

                    var result = functor.MergeAsBoolean(arguments, callStack, 2, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch
                {
                    throw new Exception("OR requires its arguments to all be Boolean");
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("or threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(or {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("!")]
        [FunctionName("not")]
        public static ILispNode Not(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                try
                {
                    var merger = new AtomMerger(new LispMissing(), (r, x) => !(bool) x);

                    var result = functor.MergeAsBoolean(arguments, callStack, 1, merger);

                    return (merger.MissingSymbols.Count() > 0)
                        ? merger.MissingSymbols
                        : result;
                }
                catch
                {
                    throw new Exception("NOT requires its single argument to be Boolean");
                }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("not threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(not {0}) done", arguments.ToLispArgString());
            }
#endif
        }
    }
}