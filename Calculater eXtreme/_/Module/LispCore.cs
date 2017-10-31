using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber.Module
{
    public static class LispCore
    {
        [FunctionName("nil")]
        public static ILispNode NIL(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            return new LispNil();
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("nil threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(nil {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("nil?")]
        [FunctionName("nil-p")]
        public static ILispNode NilP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("NIL-P requires exactly one argument");
            }

            var result = arguments[0].Eval(callStack, true);

            if (result is LispNil)
            {
                return new LispAtom(true);
            }
            if ((result is LispAtom) && !(result as LispAtom).ValueAsBoolean)
            {
                return new LispAtom(true);
            }
            if ((result is LispList) && ((result as LispList).Count == 0))
            {
                return new LispAtom(true);
            }

            return new LispAtom(false);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("nil_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(nil_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("defun")]
        public static ILispNode Defun(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            try
            {
                if (arguments.Count < 3)
                {
                    throw new Exception("DEFUN requires at least 3 arguments");
                }

                var functionName = arguments[0].Eval(callStack, false);
                if (!(functionName is LispAtom))
                {
                    throw new Exception("A defun must have a first argument which evaluates to an Atom");
                }

                var name = (functionName as LispAtom).ValueAsString;
                if (name == String.Empty)
                {
                    throw new Exception("A defun must have a non-null string for a name as its first argument");
                }

                if (!(arguments[1] is LispList))
                {
                    throw new Exception("A defun must have a LIST of formal parameters as its second argument");
                }

                var formalParams = (arguments[1] as LispList);

                callStack[name] = new Defun(name, formalParams, arguments.Skip(2).ToList());
                return new LispAtom(null, name, Token.ValidName);
            }
            catch
            {
                return new LispNil();
            }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("defun threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(defun {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("lambda")]
        public static ILispNode Lambda(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            try
            {
                if (arguments.Count < 2)
                {
                    throw new Exception("LAMBDA requires at least 2 arguments");
                }

                if (!(arguments[0] is LispList))
                {
                    throw new Exception("A lambda must have a LIST of formal parameters as its first argument");
                }

                var result = new LispList
                             {
                                 (arguments[0] as LispList)
                             };

                foreach (var argument in arguments.Skip(1))
                {
                    result.Add(argument);
                }

                return result;
            }
            catch
            {
                return new LispNil();
            }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("lambda threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(lambda {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("map")]
        public static ILispNode Map(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            try
            {
                if (arguments.Count < 1)
                {
                    throw new Exception("MAP requires at least 1 argument");
                }

                var result = new LispList();

                var mapFunctor = arguments[0].Eval(callStack, args);

                // run through the arguments list one at a time and map them to the result with the lambda
                arguments.Skip(1).ToList().ForEach(x => result.Add(Functor.Apply(mapFunctor, new LispList().Add(x), callStack, args)));

                return result;
            }
            catch
            {
                return new LispNil();
            }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("map threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(map {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("apply")]
        public static ILispNode Apply(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            try
            {
                if (arguments.Count < 1)
                {
                    throw new Exception("MAP requires at least 1 argument");
                }

                var applyFunctor = arguments[0].Eval(callStack, args);

                return Functor.Apply(applyFunctor, new LispList(null, arguments.Skip(1)), callStack, args);
            }
            catch
            {
                return new LispNil();
            }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("apply threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(apply {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("setq")]
        public static ILispNode SetQ(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 2)
            {
                throw new Exception("SETQ requires exactly 2 arguments");
            }

            var variableName = arguments[0].Eval(callStack, false);
            if (!(variableName is LispAtom))
            {
                throw new Exception("A setq must have a first argument which evaluates to an Atom");
            }

            var name = (variableName as LispAtom).ValueAsString;
            if (name == String.Empty)
            {
                throw new Exception("A setq must have a non-null string for a name as its first argument");
            }

            callStack[name] = arguments[1].Eval(callStack, true);

            return (callStack[name] as ILispNode);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("setq threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(setq {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("prog")]
        public static ILispNode Prog(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            var values = arguments.Select(x => x.Eval(callStack, true)).ToList();

            var missingSymbols = values.Aggregate(new LispMissing(), (r, x) => r.Merge(x));

            return (missingSymbols.Count() > 0)
                       ? missingSymbols
                       : values.Last();
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("prog threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(prog {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("atom")]
        public static ILispNode Atom(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("ATOM requires exactly one argument");
            }

            var result = arguments[0].Eval(callStack, true);

            if (result is LispNil)
            {
                return result;
            }
            if (result is LispMissing)
            {
                return result;
            }
            if (result is LispAtom)
            {
                return result;
            }

            throw new Exception("ATOM requires arguments that evaluate to Atom");
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("atom threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(atom {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("atom?")]
        [FunctionName("atom-p")]
        public static ILispNode AtomP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("ATOM-P requires exactly one argument");
            }

            var result = arguments[0].Eval(callStack, true);
            if (result is LispAtom)
            {
                if ((result as LispAtom).RawValue is LispList)
                {
                    return new LispAtom(false);
                }
                return new LispAtom(true);
            }

            return new LispAtom(false);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("atom_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(atom_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("list")]
        public static ILispNode List(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count < 1)
            {
                return new LispNil();
            }

            var missing = new LispMissing();
            var result = new LispList();

            foreach (var xEval in arguments.Select(x => x.Eval(callStack, true)))
            {
                if (xEval is LispMissing)
                {
                    missing.Merge(xEval);
                }
                else
                {
                    result.Add(xEval);
                }
            }

            return (missing.Count() > 0)
                       ? missing
                       : result as ILispNode;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("list threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(list {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("list?")]
        [FunctionName("list-p")]
        [FunctionName("listp")]
        public static ILispNode ListP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("LIST-P requires exactly one argument");
            }

            var result = arguments[0].Eval(callStack, true);
            if (result is LispList)
            {
                return new LispAtom(true);
            }
            if ((result is LispAtom) && ((result as LispAtom).RawValue is LispList))
            {
                return new LispAtom(true);
            }

            return new LispAtom(false);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("list_p threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(list_p {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("cons")]
        public static ILispNode Cons(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            var missing = new LispMissing();
            var result = new LispList();

            if (arguments.Count != 2)
            {
                throw new Exception("CONS requires exactly 2 arguments");
            }

            do
            {
                var car = arguments[0].Eval(callStack, true);
                if (car is LispMissing)
                {
                    missing.Merge(car);
                    break;
                }

                result.Add(car);

                var cdr = arguments[1].Eval(callStack, true);
                if (cdr is LispNil)
                {
                    break;
                }
                if ((cdr is LispAtom) && (cdr as LispAtom).IsNil)
                {
                    break;
                }

                if (cdr is LispMissing)
                {
                    missing.Merge(cdr);
                    break;
                }

                if (cdr is LispList)
                {
                    if ((cdr as LispList).IsNil)
                    {
                        break;
                    }

                    (cdr as LispList).ForEach(
                        x =>
                        {
                            try
                            {
                                var xEval = x.Eval(callStack, true);
                                missing.Merge(xEval);

                                if (xEval is LispMissing)
                                {
                                    throw new Exception();
                                }

                                result.Add(xEval);
                            }
                            catch
                            {
                                result.Add(x);
                            }
                        });
                }
                else
                {
                    result.IsImproperList = true;
                    result.Add(cdr);
                }
            } while (false);

            return (missing.Count() > 0)
                       ? missing
                       : result as ILispNode;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("cons threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(cons {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("cond")]
        [FunctionName("switch")]
        public static ILispNode Cond(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            var missing = new LispMissing();
            if (arguments.Count < 1)
            {
                throw new Exception("COND requires at least 1 argument");
            }

            for (var i = 0; i <= arguments.Count; i++)
            {
                var _case = arguments[i];
                if (_case is LispMissing)
                {
                    missing.Merge(_case);
                    continue;
                }

                if (!(_case is LispList) && ((_case as LispList).Count != 2))
                {
                    throw new Exception("Each COND case requires exactly 2 arguments");
                }

                var test = (_case as LispList)[0].Eval(callStack, true);
                if (test is LispMissing)
                {
                    missing.Merge(test);
                    continue;
                }

                if ((test is LispNil) || (!(test as LispAtom).ValueAsBoolean))
                {
                    continue;
                }

                return (_case as LispList)[1].Eval(callStack, true);
            }

            return new LispNil();
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("cond threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(cond {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("if")]
        [FunctionName("iif")]
        [FunctionName("if_then")]
        [FunctionName("if_then_else")]
        public static ILispNode Iff(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count < 2)
            {
                throw new Exception("IF requires at least 2 arguments");
            }

            if (arguments.Count > 3)
            {
                throw new Exception("IF requires at most 3 arguments");
            }

            var test = arguments[0].Eval(callStack, true);
            if (test is LispMissing)
            {
                return test;
            }

            if (test is LispList)
            {
                throw new Exception("IF requires a test which evaluates to an Atom");
            }

            var fTrue = (test as LispAtom).ValueAsBoolean;

            return fTrue
                       ? arguments[1].Eval(callStack, true)
                       : ((arguments.Count == 3)
                              ? arguments[2].Eval(callStack, true)
                              : (new LispNil()));
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("iif threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(iif {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("car")]
        public static ILispNode Car(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("CAR requires exactly 1 argument");
            }

            var arg = arguments[0].Eval(callStack, true);
            if ((arg is LispMissing) || (arg is LispNil))
            {
                return arg;
            }
            if (arg is LispAtom)
            {
                return new LispNil();
            }

            return (arg as LispList).Car;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("car threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(car {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("cdr")]
        public static ILispNode Cdr(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            if (arguments.Count != 1)
            {
                throw new Exception("CDR requires exactly 1 argument");
            }
            try
            {
                var arg = arguments[0].Eval(callStack, true);
                if (arg is LispMissing)
                {
                    return arg;
                }

                if ((arg is LispNil) || (arg is LispAtom))
                {
                    return new LispNil();
                }

                return (arg as LispList).Cdr;
            }
            catch (Exception)
            {
                return new LispNil();
            }
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("cdr threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(cdr {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("print")]
        public static ILispNode Print(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object[] args)
        {
#if TRACE_FLOW
            try
            {
#endif
            var strResult = arguments.Aggregate(
                String.Empty,
                (r, x) => r + (((r == String.Empty)
                                    ? ""
                                    : " ") + x.Eval(callStack, true).ToString()));

            Console.WriteLine(strResult);

            return new LispAtom(strResult);
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("print threw Exception: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(print {0}) done", arguments.ToLispArgString());
            }
#endif
        }
    }
}