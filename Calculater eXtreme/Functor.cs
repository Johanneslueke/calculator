//#define TRACE_FLOW_VERBOSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace BrightSword.LightSaber
{
    /// <summary>
    /// A function which folds the expression rooted at Root if possible.
    /// 
    /// Variables are resolved if appropriate using callStack.
    /// An incoming result value is merged, if possible, into the operation, and then returned if appropriate.
    /// 
    /// For example, in the event where a list is being folded and a member of the list contains an unbound symbol, that member's eval() function would return a Missing object as its result. 
    /// The next member of the list would then merge its unbound callStack, if any, into this object, and return it, instead of returning a computed value. 
    /// 
    /// Generally, the result of any folding function would either a computed value or a Missing object with a set of callStack that could not be bound
    /// </summary>
    /// <param name="root"></param>
    /// <param name="result"></param>
    /// <param name="callStack"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate ILispNode FoldFunc(ILispNode root, ILispNode result, CallStack callStack, params object [ ] args);

    /// <summary>
    /// A function which maps the element rooted at Root into another element
    /// 
    /// Values can be resolved if necessary using callStack.
    /// 
    /// </summary>
    /// <param name="root"></param>
    /// <param name="callStack"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate ILispNode MapFunc(ILispNode root, CallStack callStack, params object [ ] args);

    /// <summary>
    /// A function of this signature can be registered to provide functionality to the Eden system.
    /// 
    /// All pre-defined functions are implemented with functions conforming to this delegate.
    /// 
    /// 
    /// </summary>
    /// <param name="functor"></param>
    /// <param name="arguments"></param>
    /// <param name="callStack"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate ILispNode ApplyFunc(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args);

    internal class Functor
    {
        public Functor(Defun defun)
        {
            DynamicFunction = defun;
            NativeFunctions = null;
        }

        public Functor(List<ApplyFunc> nativeFunctions)
        {
            NativeFunctions = nativeFunctions;
            DynamicFunction = null;
        }

        protected Defun DynamicFunction { get; set; }
        protected List<ApplyFunc> NativeFunctions { get; set; }

        protected static Functor ResolveFunctor(string functionName, CallStack callStack, params object [ ] args)
        {
            // stack walk to see if there is any scoped defun matching this name
            var defun = callStack[functionName] as Defun;
            if (defun != null)
            {
                return new Functor(defun);
            }

            // return a (possibly zero-length) list of functions registered under this name
            return new Functor(Library.GetRegisteredFunctionsByName(functionName));
        }

        private static ILispNode ReplaceParamName(ILispNode root, string paramName, string newParamName)
        {
            if (root == null)
            {
                return root;
            }
            if (root is LispNil)
            {
                return root;
            }
            if (root is LispMissing)
            {
                return root;
            }

            if (root is LispAtom)
            {
                return (string.Compare((root as LispAtom).TokenValue, paramName, true) == 0)
                    ? new LispAtom(null, newParamName, Token.ValidName)
                    : root;
            }

            return new LispList(null, (root as LispList).Select(x => ReplaceParamName(x, paramName, newParamName)).ToList());
        }

        public static ILispNode ApplyDynamicFunction(Lambda lambda, LispList arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                callStack.PushFrame();

                var functionBodyStatements = lambda.Body;
                var fOptional = false;

                for (var iParam = 0; iParam < lambda.FormalParameters.Count; iParam++)
                {
                    var param = lambda.FormalParameters[iParam];
                    Debug.Assert(param is LispAtom, "Formal Parameter should be an Atom");

                    var paramName = (param as LispAtom).ValueAsString;
                    Debug.Assert(paramName != String.Empty, "Formal Parameter name cannot be a null string");

                    fOptional |= paramName.EndsWith("?");
                    paramName = paramName.TrimEnd('?');

                    ILispNode paramValue = null;
                    try
                    {
                        paramValue = arguments[iParam].Eval(callStack, true);
                        if (paramValue is LispMissing)
                        {
                            throw new Exception("Value to be bound cannot be evaluated");
                        }
                    }
                    catch
                    {
                        if (!fOptional)
                        {
                            throw new Exception(String.Format("Cannot find value to bind to {0}", paramName));
                        }
                        paramValue = new LispNil();
                    }
                    finally
                    {
                        Debug.Assert(paramValue != null, paramName + " is bound to null");

                        var invocationParamName = String.Format("{0}_{1}", paramName, callStack.NumberOfFrames);
                        callStack[invocationParamName] = paramValue;

                        functionBodyStatements = functionBodyStatements.Select(x => ReplaceParamName(x, paramName, invocationParamName)).ToList();
                    }
                }

                ILispNode result = new LispNil();
                foreach (var statement in functionBodyStatements)
                {
                    result = statement.Eval(callStack);
                }
                return result;
            }
            finally
            {
                callStack.PopFrame();
            }
        }

        public static ILispNode Apply(ILispNode functor, LispList arguments, CallStack callStack, params object [ ] args)
        {
            if (functor is LispNil)
            {
                return new LispNil();
            }

            if (functor is LispMissing)
            {
                throw new Exception("Cannot apply a Functor of type Missing!");
            }

            Lambda lambda = null;
            do
            {
                if (functor is LispAtom)
                {
                    var functionName = (functor as LispAtom).ValueAsString;
                    if (functionName == String.Empty)
                    {
                        throw new Exception("functor's ValueAsString is Empty!");
                    }

                    var resolvedFunctor = ResolveFunctor(functionName, callStack, args);
                    if (resolvedFunctor.DynamicFunction != null)
                    {
                        lambda = resolvedFunctor.DynamicFunction;
                        break;
                    }

                    foreach (var nativeFunction in resolvedFunctor.NativeFunctions)
                    {
                        try
                        {
#if TRACE_FLOW
                            var strTrace = String.Format("{0}({1} {2})", new String(' ', callStack.NumberOfFrames), functionName, arguments.ToString().Unlist());
                            _calls.Push(strTrace);
#endif
                            var result = nativeFunction.Invoke(functor, arguments, callStack, args);
#if TRACE_FLOW
#if TRACE_FLOW_VERBOSE
                        Console.WriteLine("{0} => {1}\n{2}", Calls.Pop(), result.ToString(), callStack.ToString());
#else
                            Console.WriteLine("{0} => {1}", _calls.Pop(), result);
#endif
#endif
                            return result;
                        }
#if TRACE_FLOW
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                "({0} {1}) throws: {2}",
                                nativeFunction.GetType().FullName + "." + nativeFunction.Method.Name,
                                arguments.ToLispArgString(),
                                ex.Message);
                            Console.WriteLine("Stack:\n{0}", _calls.ToList().Aggregate(String.Empty, (r, x) => r + ("\n" + x)));
                            continue;
                        }
#else
                        catch
                        {
                            continue;
                        }
#endif
                    }
                }

                if (functor is LispList)
                {
                    var lambdaExpression = (functor as LispList);
                    lambda = new Lambda(lambdaExpression.Car as LispList, lambdaExpression.Cdr as IList<ILispNode>);
                }
            }
            while (false);

            try
            {
                return ApplyDynamicFunction(lambda, arguments, callStack, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("({0} {1}) throws: {2}", lambda, arguments.ToLispArgString(), ex.Message);
            }

            throw new LispException(functor, String.Format("{0} : All known implementations have failed: ", functor));
        }
    }

    internal class Lambda
    {
        public Lambda(LispList formalParameters, IList<ILispNode> body)
        {
            FormalParameters = formalParameters;
            Body = body;
        }

        public LispList FormalParameters { get; private set; }

        public IList<ILispNode> Body { get; private set; }

        public override string ToString()
        {
            return String.Format("(lambda ({0}) ({1}))", FormalParameters.ToLispArgString(), Body.ToLispArgString());
        }
    }

    internal class Defun : Lambda
    {
        public Defun(string functionName, LispList formalParameters, IList<ILispNode> body) : base(formalParameters, body)
        {
            FunctionName = functionName;
        }

        public string FunctionName { get; private set; }
    }

    internal delegate object MergeFunc(object result, object value);

    internal interface IMerger
    {
        MergeFunc MergeMissing { get; }
        MergeFunc MergeNil { get; }
        MergeFunc MergeList { get; }
        MergeFunc MergeAtom { get; }
    }

    internal class AtomMerger : IMerger
    {
        public AtomMerger(LispMissing missingSymbols, MergeFunc mergeAtom)
        {
            MissingSymbols = missingSymbols;
            MergeAtom = mergeAtom;
        }

        public AtomMerger(LispMissing missingSymbols) : this(missingSymbols, (r, x) => r) {}
        public LispMissing MissingSymbols { get; private set; }

        #region IMerger Members

        public MergeFunc MergeAtom { get; private set; }

        public MergeFunc MergeMissing
        {
            get
            {
                return ((result, value) =>
                {
                    MissingSymbols.Merge(value as LispMissing);
                    return result;
                });
            }
        }

        public MergeFunc MergeNil { get { return ((result, value) => result); } }

        public MergeFunc MergeList { get { return ((result, value) => {throw new Exception("Cannot operate on Lists");}); } }

        #endregion
    }

    internal static class MergeHelper
    {
        public static ILispNode MergeAsNumber(this ILispNode root, IList<ILispNode> arguments, CallStack callStack, int arity, IMerger merger)
        {
            if (arguments.Count < arity)
            {
                throw new Exception("Not enough arguments");
            }

            var fFirst = true;
            var fUnary = (arity == 1);

            return new LispAtom(
                arguments.Aggregate(
                    double.NaN,
                    (result, xArg) =>
                    {
                        var xEval = xArg.Eval(callStack, true);

                        if ((xEval is LispNil) && (merger.MergeNil != null))
                        {
                            return (double) merger.MergeNil(result, xEval as LispNil);
                        }

                        if ((xEval is LispMissing) && (merger.MergeMissing != null))
                        {
                            return (double) merger.MergeMissing(result, xEval as LispMissing);
                        }

                        if ((xEval is LispList) && (merger.MergeList != null))
                        {
                            return (double) merger.MergeList(result, xEval as LispList);
                        }

                        Debug.Assert(xEval is LispAtom, "Argument does not evaluate to an Atom!");

                        try
                        {
                            if (!(xEval as LispAtom).IsNumber)
                            {
                                throw new Exception();
                            }

                            // if fUnary, it means the function has only the first argument, which must be merged
                            // otherwise, we can prime the pump with the value of the first argument
                            if (fFirst && !fUnary)
                            {
                                result = (xEval as LispAtom).ValueAsNumber;
                                fFirst = false;

                                return result;
                            }
                            return (double) merger.MergeAtom(result, (xEval as LispAtom).ValueAsNumber);
                        }
                        catch
                        {
                            throw new Exception("Argument does not evaluate to a Number");
                        }
                    }));
        }

        public static ILispNode MergeAsString(this ILispNode root, IList<ILispNode> arguments, CallStack callStack, int arity, IMerger merger)
        {
            if (arguments.Count < arity)
            {
                throw new Exception("Not enough arguments");
            }

            var fFirst = true;
            var fUnary = (arity == 1);

            return new LispAtom(
                arguments.Aggregate(
                    string.Empty,
                    (result, xArg) =>
                    {
                        var xEval = xArg.Eval(callStack);

                        if ((xEval is LispNil) && (merger.MergeNil != null))
                        {
                            return (string) merger.MergeNil(result, xEval as LispNil);
                        }

                        if ((xEval is LispMissing) && (merger.MergeMissing != null))
                        {
                            return (string) merger.MergeMissing(result, xEval as LispMissing);
                        }

                        if ((xEval is LispList) && (merger.MergeList != null))
                        {
                            return (string) merger.MergeList(result, xEval as LispList);
                        }

                        Debug.Assert(xEval is LispAtom, "Argument does not evaluate to an Atom!");

                        try
                        {
                            if (!(xEval as LispAtom).IsString)
                            {
                                throw new Exception();
                            }

                            // if fUnary, it means the function has only the first argument, which must be merged
                            // otherwise, we can prime the pump with the value of the first argument
                            if (fFirst && !fUnary)
                            {
                                fFirst = false;

                                return (xEval as LispAtom).ValueAsString;
                            }
                            return (string) merger.MergeAtom(result, (xEval as LispAtom).ValueAsString);
                        }
                        catch
                        {
                            throw new Exception("Argument does not evaluate to a String");
                        }
                    }));
        }

        public static ILispNode MergeAsBoolean(this ILispNode root, IList<ILispNode> arguments, CallStack callStack, int arity, IMerger merger)
        {
            if (arguments.Count < arity)
            {
                throw new Exception("Not enough arguments");
            }

            var fFirst = true;
            var fUnary = (arity == 1);

            return new LispAtom(
                arguments.Aggregate(
                    false,
                    (result, xArg) =>
                    {
                        var xEval = xArg.Eval(callStack);

                        // nil is treated as false
                        if (xEval is LispNil)
                        {
                            xEval = new LispAtom(false);
                        }

                        if ((xEval is LispMissing) && (merger.MergeMissing != null))
                        {
                            return (bool) merger.MergeMissing(result, xEval as LispMissing);
                        }

                        if ((xEval is LispList) && (merger.MergeList != null))
                        {
                            return (bool) merger.MergeList(result, xEval as LispList);
                        }

                        Debug.Assert(xEval is LispAtom, "Argument does not evaluate to an Atom!");

                        try
                        {
                            if (!(xEval as LispAtom).IsBoolean)
                            {
                                throw new Exception();
                            }

                            // if fUnary, it means the function has only the first argument, which must be merged
                            // otherwise, we can prime the pump with the value of the first argument
                            if (fFirst && !fUnary)
                            {
                                result = (xEval as LispAtom).ValueAsBoolean;
                                fFirst = false;

                                return result;
                            }
                            return (bool) merger.MergeAtom(result, (xEval as LispAtom).ValueAsBoolean);
                        }
                        catch
                        {
                            throw new Exception("Argument does not evaluate to a Boolean");
                        }
                    }));
        }

        public static ILispNode MergeAsDateTime(this ILispNode root, IList<ILispNode> arguments, CallStack callStack, int arity, IMerger merger)
        {
            if (arguments.Count < arity)
            {
                throw new Exception("Not enough arguments");
            }

            var fFirst = true;
            var fUnary = (arity == 1);

            return new LispAtom(
                arguments.Aggregate(
                    DateTime.MinValue,
                    (result, xArg) =>
                    {
                        var xEval = xArg.Eval(callStack, true);

                        if ((xEval is LispNil) && (merger.MergeNil != null))
                        {
                            return (DateTime) merger.MergeNil(result, xEval as LispNil);
                        }

                        if ((xEval is LispMissing) && (merger.MergeMissing != null))
                        {
                            return (DateTime) merger.MergeMissing(result, xEval as LispMissing);
                        }

                        if ((xEval is LispList) && (merger.MergeList != null))
                        {
                            return (DateTime) merger.MergeList(result, xEval as LispList);
                        }

                        Debug.Assert(xEval is LispAtom, "Argument does not evaluate to an Atom!");

                        try
                        {
                            // if fUnary, it means the function has only the first argument, which must be merged
                            // otherwise, we can prime the pump with the value of the first argument
                            if (fFirst && !fUnary)
                            {
                                if (!(xEval as LispAtom).IsDateTime)
                                {
                                    throw new Exception("The first argument to DateTime functions should be of type DateTime");
                                }

                                result = (xEval as LispAtom).ValueAsDateTime;
                                fFirst = false;

                                return result;
                            }
                            
                            try
                            {
                                var ts = TimeSpan.Parse((xEval as LispAtom).ValueAsString);
                                return (DateTime) merger.MergeAtom(result, ts);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Subsequent arguments to DateTime functions should be of type TimeSpan", ex);
                            }
                        }
                        catch
                        {
                            throw new Exception("Argument does not evaluate to a DateTime");
                        }
                    }));
        }
    }

    internal delegate bool ItemTestFunc(ILispNode value);

    internal delegate object FirstItemFunc(ILispNode curr);

    internal delegate object SequenceFunc(object result, ILispNode prev, ILispNode curr);

    internal interface IWalker
    {
        ItemTestFunc ItemTest { get; }
        FirstItemFunc First { get; }
        SequenceFunc Sequence { get; }
    }

    internal class DateTimeWalker : IWalker
    {
        public DateTimeWalker(SequenceFunc sequence)
        {
            Sequence = sequence;
        }

        public DateTimeWalker(string op)
        {
            switch (op.ToLower())
            {
                case "==":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime == (c as LispAtom).ValueAsDateTime);};
                }
                    break;
                case ">":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime > (c as LispAtom).ValueAsDateTime);};
                }
                    break;
                case ">=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime >= (c as LispAtom).ValueAsDateTime);};
                }
                    break;
                case "<":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime < (c as LispAtom).ValueAsDateTime);};
                }
                    break;
                case "<=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime <= (c as LispAtom).ValueAsDateTime);};
                }
                    break;
                case "!=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsDateTime != (c as LispAtom).ValueAsDateTime);};
                }
                    break;
            }
        }

        public DateTimeWalker() : this((result, prev, curr) => result) {}

        #region IWalker Members

        public ItemTestFunc ItemTest
        {
            get
            {
                return xEval =>
                {
                    if (!((xEval as LispAtom).IsDateTime))
                    {
                        throw new Exception("Value is not a DateTime");
                    }
                    return true;
                };
            }
        }

        public FirstItemFunc First { get { return first => (first as LispAtom).ValueAsDateTime; } }

        public SequenceFunc Sequence { get; private set; }

        #endregion
    }

    internal class NumberWalker : IWalker
    {
        public NumberWalker(SequenceFunc sequence)
        {
            Sequence = sequence;
        }

        public NumberWalker(string op)
        {
            switch (op.ToLower())
            {
                case "==":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber == (c as LispAtom).ValueAsNumber);};
                }
                    break;
                case ">":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber > (c as LispAtom).ValueAsNumber);};
                }
                    break;
                case ">=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber >= (c as LispAtom).ValueAsNumber);};
                }
                    break;
                case "<":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber < (c as LispAtom).ValueAsNumber);};
                }
                    break;
                case "<=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber <= (c as LispAtom).ValueAsNumber);};
                }
                    break;
                case "!=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsNumber != (c as LispAtom).ValueAsNumber);};
                }
                    break;
            }
        }

        public NumberWalker() : this((result, prev, curr) => result) {}

        #region IWalker Members

        public ItemTestFunc ItemTest
        {
            get
            {
                return xEval =>
                {
                    if (!((xEval as LispAtom).IsNumber))
                    {
                        throw new Exception("Value is not a Number");
                    }
                    return true;
                };
            }
        }

        public FirstItemFunc First { get { return first => (first as LispAtom).ValueAsNumber; } }

        public SequenceFunc Sequence { get; private set; }

        #endregion
    }

    internal class BooleanWalker : IWalker
    {
        public BooleanWalker(SequenceFunc sequence)
        {
            Sequence = sequence;
        }

        public BooleanWalker(string op)
        {
            switch (op.ToLower())
            {
                case "==":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsBoolean == (c as LispAtom).ValueAsBoolean);};
                }
                    break;
                case ">":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsBoolean && (!(c as LispAtom).ValueAsBoolean));};
                }
                    break;
                case ">=":
                {
                    Sequence = (r, p, c) => {return (bool) r && (p as LispAtom).ValueAsBoolean;};
                }
                    break;
                case "<":
                {
                    Sequence = (r, p, c) => {return (bool) r && (!(p as LispAtom).ValueAsBoolean && (c as LispAtom).ValueAsBoolean);};
                }
                    break;
                case "<=":
                {
                    Sequence = (r, p, c) => {return (bool) r && !(p as LispAtom).ValueAsBoolean;};
                }
                    break;
                case "!=":
                {
                    Sequence = (r, p, c) => {return (bool) r && ((p as LispAtom).ValueAsBoolean != (c as LispAtom).ValueAsBoolean);};
                }
                    break;
            }
        }

        public BooleanWalker() : this((result, prev, curr) => result) {}

        #region IWalker Members

        public ItemTestFunc ItemTest
        {
            get
            {
                return xEval =>
                {
                    if (!((xEval as LispAtom).IsBoolean))
                    {
                        throw new Exception("Value is not a Boolean");
                    }
                    return true;
                };
            }
        }

        public FirstItemFunc First { get { return first => (first as LispAtom).ValueAsBoolean; } }

        public SequenceFunc Sequence { get; private set; }

        #endregion
    }

    internal class StringWalker : IWalker
    {
        public StringWalker(SequenceFunc sequence)
        {
            Sequence = sequence;
        }

        public StringWalker(string op)
        {
            switch (op.ToLower())
            {
                case "==":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) == 0);};
                }
                    break;
                case ">":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) > 0);};
                }
                    break;
                case ">=":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) >= 0);};
                }
                    break;
                case "<":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) < 0);};
                }
                    break;
                case "<=":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) <= 0);};
                }
                    break;
                case "!=":
                {
                    Sequence =
                        (r, p, c) => {return (bool) r && (String.Compare((p as LispAtom).ValueAsString, (c as LispAtom).ValueAsString, true) != 0);};
                }
                    break;
            }
        }

        public StringWalker() : this((result, prev, curr) => result) {}

        #region IWalker Members

        public ItemTestFunc ItemTest
        {
            get
            {
                return xEval =>
                {
                    if (!((xEval as LispAtom).IsString))
                    {
                        throw new Exception("Value is not a String");
                    }
                    return true;
                };
            }
        }

        public FirstItemFunc First { get { return first => (first as LispAtom).ValueAsString; } }

        public SequenceFunc Sequence { get; private set; }

        #endregion
    }

    internal static class StringHelper
    {
        public static string Quote(this string _this)
        {
            var fContainsWhitespace = Regex.IsMatch(_this, ".*[\\s]+.*");

            return fContainsWhitespace
                ? String.Format("'{0}'", _this)
                : _this;
        }

        public static string Unquote(this string _this)
        {
            if ((_this[0] == '\'') && (_this[_this.Length - 1] == '\''))
            {
                return _this.Substring(1, _this.Length - 2);
            }
            if ((_this[0] == '"') && (_this[_this.Length - 1] == '"'))
            {
                return _this.Substring(1, _this.Length - 2);
            }

            return _this;
        }

        public static string Unlist(this string _this)
        {
            if ((_this[0] == '(') && (_this[_this.Length - 1] == ')'))
            {
                return _this.Substring(1, _this.Length - 2);
            }

            return _this;
        }
    }

    internal static class ListHelper
    {
        public static string ToLispArgString(this IList<ILispNode> _this)
        {
            return _this.Aggregate(
                String.Empty,
                (r, x) => r + (((r == String.Empty)
                    ? ""
                    : " ") + x.ToString()));
        }
    }

    internal static class WalkHelper
    {
        public static ILispNode WalkAsBoolean(this ILispNode root, IList<ILispNode> arguments, CallStack callStack, int arity, IMerger merger, IWalker walker)
        {
            ILispNode xPrev = null;

            return new LispAtom(
                arguments.Aggregate(
                    true,
                    (result, xArg) =>
                    {
                        var xEval = xArg.Eval(callStack);

                        if ((xEval is LispNil) && (merger.MergeNil != null))
                        {
                            return (bool) merger.MergeNil(result, xEval as LispNil);
                        }

                        if ((xEval is LispMissing) && (merger.MergeMissing != null))
                        {
                            return (bool) merger.MergeMissing(result, xEval as LispMissing);
                        }

                        if ((xEval is LispList) && (merger.MergeList != null))
                        {
                            return (bool) merger.MergeList(result, xEval as LispList);
                        }

                        try
                        {
                            Debug.Assert(xEval is LispAtom, "Argument does not evaluate to an Atom!");

                            if (walker.ItemTest(xEval))
                            {
                                if (xPrev == null)
                                {
                                    walker.First(xEval);
                                    return true;
                                }
                                else
                                {
                                    return (bool) walker.Sequence(result, xPrev, xEval);
                                }
                            }

                            throw new Exception("ItemTest failed without throwing an exception!");
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Argument is invalid", ex);
                        }
                        finally
                        {
                            xPrev = xEval;
                        }
                    }));
        }
    }
}