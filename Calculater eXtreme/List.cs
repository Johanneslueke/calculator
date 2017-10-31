using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber
{
    public interface ILispNode
    {
        /// <summary>
        /// The parent node of this instance.
        /// </summary>
        LispList Parent { get; }

        object Value { get; }

        ILispNode SetValue(object value);

        ILispNode Eval(CallStack callStack, params object [ ] args);

        ILispNode Clone();
    }

    public class LispNil : ILispNode
    {
        #region ILispNode Members

        /// <summary>
        /// The parent node of this instance.
        /// </summary>
        public virtual LispList Parent { get; protected set; }

        public virtual object Value { get { return this; } }

        public virtual ILispNode SetValue(object value)
        {
            return this;
        }

        public virtual ILispNode Eval(CallStack callStack, params object [ ] args)
        {
            return this;
        }

        public ILispNode Clone()
        {
            return new LispNil();
        }

        #endregion

        public override string ToString()
        {
            return "nil";
        }
    }

    public class LispMissing : ILispNode, IEnumerable<string>
    {
        private readonly HashSet<string> _missingSymbols = new HashSet<string>();

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return _missingSymbols.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _missingSymbols.GetEnumerator();
        }

        #endregion

        #region ILispNode Members

        public LispList Parent { get; protected set; }

        public object Value { get { return this; } }

        public ILispNode SetValue(object value)
        {
            throw new NotSupportedException();
        }

        public virtual ILispNode Eval(CallStack callStack, params object [ ] args)
        {
            return this;
        }

        public ILispNode Clone()
        {
            var result = new LispMissing();
            return result.Merge(this);
        }

        #endregion

        public override string ToString()
        {
            return String.Format(
                "[{0}]",
                this.Aggregate(
                    String.Empty,
                    (r, x) => r + (((r == String.Empty)
                        ? ""
                        : " ") + x.ToString())));
        }

        public LispMissing Register(string symbol)
        {
            _missingSymbols.Add(symbol);
            return this;
        }

        public LispMissing Unregister(string symbol)
        {
            _missingSymbols.Remove(symbol);
            return this;
        }

        public LispMissing Clear()
        {
            _missingSymbols.Clear();
            return this;
        }

        public LispMissing Merge(ILispNode source)
        {
            try
            {
                if (source is LispMissing)
                {
                    (source as LispMissing).ToList().ForEach(x => _missingSymbols.Add(x));
                }
                return this;
            }
            catch
            {
                return new LispMissing();
            }
        }
    }

    public sealed class LispAtom : ILispNode
    {
        public LispAtom(LispList parent, string tokenValue, Token token)
        {
            Parent = parent;
            TokenValue = tokenValue;
            Token = token;

            SetValue(tokenValue as object);

            if (Parent != null)
            {
                Parent.Add(this);
            }
        }

        public LispAtom(LispList parent, object value) : this(parent, String.Empty, Token.MAX)
        {
            SetValue(value);
        }

        public LispAtom(object value) : this(null, String.Empty, Token.MAX)
        {
            SetValue(value);
        }

        private bool EvalComplete { get; set; }

        public bool IsNil
        {
            get
            {
                if (RawValue == null)
                {
                    return true;
                }
                if (RawValue is LispNil)
                {
                    return true;
                }
                if (RawValue.ToString().ToLower() == "nil")
                {
                    return true;
                }
                if (RawValue is LispAtom)
                {
                    return (RawValue as LispAtom).IsNil;
                }
                if (RawValue is LispList)
                {
                    return (RawValue as LispList).IsNil;
                }
                return false;
            }
        }

        /// <summary>
        /// The raw value of the expression as it was evaluated and put in
        /// </summary>
        public object RawValue { get; private set; }

        public bool IsBoolean
        {
            get
            {
                try
                {
                    CastToBoolean(RawValue);

                    // no exception thrown - this can be viewed as a boolean
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool ValueAsBoolean
        {
            get
            {
                try
                {
                    return CastToBoolean(RawValue);
                }
                catch
                {
                    return false;
                }
            }

            set { RawValue = value; }
        }

        public bool IsNumber
        {
            get
            {
                try
                {
                    switch (Token)
                    {
                        case Token.DoubleQuotedText:
                        case Token.SingleQuotedText:
                        {
                            return false;
                        }
                        default:
                        {
                            CastToNumber(RawValue);

                            // no exception thrown - this can be viewed as a Number
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public double ValueAsNumber
        {
            get
            {
                try
                {
                    switch (Token)
                    {
                        case Token.DoubleQuotedText:
                        case Token.SingleQuotedText:
                        {
                            return double.NaN;
                        }
                        default:
                        {
                            return CastToNumber(RawValue);
                        }
                    }
                }
                catch
                {
                    return double.NaN;
                }
            }

            set { RawValue = value; }
        }

        public bool IsDateTime
        {
            get
            {
                try
                {
                    CastToDateTime(RawValue);

                    // no exception thrown - this can be viewed as a DateTime
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public DateTime ValueAsDateTime
        {
            get
            {
                try
                {
                    return CastToDateTime(RawValue);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }

            set { RawValue = value; }
        }

        public bool IsString
        {
            get
            {
                try
                {
                    return (RawValue is string);
                }
                catch
                {
                    return false;
                }
            }
        }

        public string ValueAsString
        {
            get
            {
                try
                {
                    return CastToString(RawValue);
                }
                catch
                {
                    return String.Empty;
                }
            }

            set { RawValue = value; }
        }

        /// <summary>
        /// The token enumeration that triggered the creation of this node.
        /// </summary>
        public Token Token { get; private set; }

        /// <summary>
        /// The value of the token that triggered the creation of this node.
        /// </summary>
        public string TokenValue { get; private set; }

        #region ILispNode Members

        /// <summary>
        /// The parent node of this instance.
        /// </summary>
        public LispList Parent { get; private set; }

        public object Value
        {
            get
            {
                if (IsDateTime)
                {
                    return ValueAsDateTime;
                }
                if (IsBoolean)
                {
                    return ValueAsBoolean;
                }
                if (IsNumber)
                {
                    return ValueAsNumber;
                }
                if (IsString)
                {
                    return ValueAsString;
                }

                return RawValue;
            }
        }

        public ILispNode SetValue(object value)
        {
            if (value is LispList)
            {
                RawValue = value;
                return (value as LispList);
            }

            if (value is LispNil)
            {
                RawValue = value;
                return (value as LispNil);
            }

            if (value is LispAtom)
            {
                return Copy(value as LispAtom);
            }

            // [true] can be coerced to a number if we don't do this 
            if ((value is long) || (value is double))
            {
                return SetValue(CastToNumber(value));
            }

            if (value is bool)
            {
                return SetValue(CastToBoolean(value));
            }

            if (value is DateTime)
            {
                return SetValue(CastToDateTime(value));
            }

            try
            {
                return SetValue(CastToNumber(value));
            }
            catch {}

            try
            {
                return SetValue(CastToBoolean(value));
            }
            catch {}

            try
            {
                return SetValue(CastToDateTime(value));
            }
            catch {}

            try
            {
                return SetValue(CastToString(value));
            }
            catch {}

            {
                RawValue = value;
                return this;
            }
        }

        public ILispNode Eval(CallStack callStack, params object [ ] args)
        {
            if (EvalComplete)
            {
                return this;
            }

            EvalComplete = true;

            var fResolve = (args.Length > 0)
                ? (bool) args[0]
                : true;

            if (IsNil)
            {
                return new LispNil();
            }

            switch (Token)
            {
                case Token.ValidNumber:
                {
                    return SetValue(TokenValue);
                }

                case Token.Text:
                case Token.SingleQuotedText:
                case Token.DoubleQuotedText:
                {
                    return SetValue(TokenValue);
                }

                case Token.ValidName:
                {
                    if (fResolve)
                    {
                        switch (TokenValue.ToLower())
                        {
                            case "true":
                            case "t":
                            case "#t":
                            {
                                return SetValue(true);
                            }

                            case "f":
                            case "#f":
                            case "false":
                            {
                                return SetValue(false);
                            }

                            case "nil":
                            {
                                return new LispNil();
                            }

                            case "today":
                            {
                                return SetValue(DateTime.Today);
                            }
                            case "now":
                            {
                                return SetValue(DateTime.Now);
                            }

                            default:
                                break;
                        }
                    }
                    break;
                }

                case Token.MAX:
                {
                    return SetValue(RawValue);
                }
            }

            try
            {
                if (fResolve)
                {
                    var resolvedValue = callStack[TokenValue];
                    if (resolvedValue != null)
                    {
                        return SetValue(resolvedValue);
                    }

                    throw new KeyNotFoundException();
                }

                return SetValue(TokenValue);
            }
            catch (KeyNotFoundException)
            {
                EvalComplete = false;
                return (new LispMissing()).Register(TokenValue);
            }
        }

        public ILispNode Clone()
        {
            return new LispAtom(null, TokenValue, Token).Copy(this);
        }

        #endregion

        internal static bool CastToBoolean(object value)
        {
            try
            {
                if (value is LispAtom)
                {
                    return CastToBoolean((value as LispAtom).RawValue);
                }

                if ((value is double) || (value is long))
                {
                    throw new Exception("Value is a declared Number!");
                }

                /*
                if (value is string)
                {
                    if (((string)value).ToLower() == "t") { return true; }
                    if (((string)value).ToLower() == "true") { return true; }
                    if (((string)value).ToLower() == "f") { return false; }
                    if (((string)value).ToLower() == "false") { return false; }
                }
                 */

                return Convert.ToBoolean(value);
            }
            catch
            {
                throw new InvalidCastException("Cannot express (" + value + ") as a Boolean");
            }
        }

        public ILispNode SetValue(bool value)
        {
            ValueAsBoolean = value;
            return this;
        }

        internal static double CastToNumber(object value)
        {
            try
            {
                if (value is LispAtom)
                {
                    return CastToNumber((value as LispAtom).RawValue);
                }

                return Convert.ToDouble(value);
            }
            catch
            {
                throw new InvalidCastException("Cannot express (" + value + ") as a Number");
            }
        }

        public ILispNode SetValue(double value)
        {
            ValueAsNumber = value;
            return this;
        }

        internal static DateTime CastToDateTime(object value)
        {
            try
            {
                if (value is LispAtom)
                {
                    return CastToDateTime((value as LispAtom).RawValue);
                }

                return Convert.ToDateTime(value);
            }
            catch
            {
                throw new InvalidCastException("Cannot express (" + value + ") as a DateTime");
            }
        }

        public ILispNode SetValue(DateTime value)
        {
            ValueAsDateTime = value;
            return this;
        }

        internal static string CastToString(object value)
        {
            try
            {
                if (value is LispAtom)
                {
                    return CastToString((value as LispAtom).RawValue);
                }

                return Convert.ToString(value);
            }
            catch
            {
                throw new InvalidCastException("Cannot express (" + value + ") as a String");
            }
        }

        public ILispNode SetValue(string value)
        {
            ValueAsString = value;
            return this;
        }

        public LispAtom Copy(LispAtom source)
        {
            RawValue = source.RawValue;
            Token = source.Token;
            TokenValue = source.TokenValue;
            EvalComplete = source.EvalComplete;

            return this;
        }

        /// <summary>
        /// Serializes the expression tree rooted at this instance into a string.
        /// </summary>
        /// <returns>A string representation of the tree</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public sealed class LispList : List<ILispNode>, ILispNode
    {
        private static readonly LispNil _nil = new LispNil();
        public LispList() {}

        public LispList(LispList parent, IEnumerable<ILispNode> value) : base(value)
        {
            Parent = parent;
        }

        public LispList(LispList parent)
        {
            Parent = parent;

            if (Parent != null)
            {
                Parent.Add(this);
            }
        }

        public bool IsNil { get { return (Count == 0); } }
        public bool IsImproperList { get; set; }

        public ILispNode Car
        {
            get
            {
                try
                {
                    return this[0].Clone();
                }
                catch
                {
                    return _nil;
                }
            }
        }

        public ILispNode Cdr
        {
            get
            {
                try
                {
                    return (IsImproperList)
                        ? (this[1]).Clone()
                        : (new LispList(Parent, this.Skip(1))).Clone();
                }
                catch
                {
                    return _nil;
                }
            }
        }

        /// <summary>
        /// The raw value of the expression as it was evaluated and put in
        /// </summary>
        public ILispNode RawValue { get; private set; }

        #region ILispNode Members

        /// <summary>
        /// The parent node of this instance.
        /// </summary>
        public LispList Parent { get; private set; }

        public object Value { get { return this; } }

        public ILispNode SetValue(object value)
        {
            if (value is LispMissing)
            {
                return SetValue(value as LispMissing);
            }
            if (value is LispNil)
            {
                return SetValue(value as LispNil);
            }
            if (value is LispAtom)
            {
                return SetValue(value as LispAtom);
            }
            if (value is LispList)
            {
                return SetValue(value as LispList);
            }

            return SetValue(new LispAtom(this, value));
        }

        public ILispNode Eval(CallStack callStack, params object [ ] args)
        {
            try
            {
                // ()
                if (Count == 0)
                {
                    return _nil;
                }

                var functor = this[0].Eval(callStack, false);
                // ( =>nil )
                if (functor is LispNil)
                {
                    return _nil;
                }
                // ( =>missing )
                if (functor is LispMissing)
                {
                    return functor;
                }

                var arguments = new LispList();
                do
                {
                    if (IsImproperList)
                    {
                        arguments.Add(this[1] as LispAtom);
                        continue;
                    }

                    arguments = new LispList(null, this.Skip(1));
                }
                while (false);

                // ( =>(lambda) ) and ( =>'funcname' )
                return Functor.Apply(functor, arguments, callStack, args);
            }
            catch (LispException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LispException(this, "Exception while applying functor to arguments", ex);
            }
        }

        public ILispNode Clone()
        {
            var result = new LispList();
            ForEach(x => result.Add(x.Clone()));

            return result;
        }

        #endregion

        private ILispNode SetValue(ILispNode value)
        {
            if (value is LispMissing)
            {
                if (RawValue is LispMissing)
                {
                    RawValue = new LispMissing();
                }

                return (RawValue as LispMissing).Merge(value as LispMissing);
            }

            RawValue = value;
            return this;
        }

        public new LispList Add(ILispNode value)
        {
            base.Add(value);
            return this;
        }

        public new LispList Remove(ILispNode value)
        {
            base.Remove(value);
            return this;
        }

        public override string ToString()
        {
            if (IsImproperList)
            {
                try
                {
                    return String.Format("({0} . {1})", this[0], this[1]);
                }
                catch
                {
                    return String.Format("[illegal cons cell]");
                }
            }
            return String.Format(
                "({0})",
                this.Aggregate(
                    String.Empty,
                    (r, x) => r + (((r == String.Empty)
                        ? ""
                        : " ") + x.ToString())));
        }

        public ILispNode Map(MapFunc map, CallStack callStack, params object [ ] args)
        {
            var rgMapped = this.Select(x => map(x, callStack, args));

            var missing = rgMapped.Aggregate(
                new LispMissing(),
                (r, x) => (x is LispMissing)
                    ? r.Merge(x as LispMissing)
                    : r);
            if (missing.Count() > 0)
            {
                return missing;
            }

            return new LispList(null, rgMapped);
        }
    }

    /// <summary>
    /// An instance of this class thrown from within the Eden library to signal operational failure
    /// </summary>
    public class LispException : Exception
    {
        internal LispException(ILispNode context, string message) : base(GetMessage(context, message)) {}

        internal LispException(ILispNode context, string message, Exception inner) : base(GetMessage(context, message), inner) {}

        private static string GetMessage(ILispNode context, string message)
        {
            return ((context == null)
                ? message
                : String.Format("{0} : \n{1}", message, context));
        }
    }

    public static class ListBuilder
    {
        /// <summary>
        /// Main entry point to the Expression class
        /// 
        /// Parses a given expression string into an expression tree, with this as the root.
        /// 
        /// </summary>
        public static ILispNode Parse(this string _this)
        {
            return ((ListBuilderContext) _this.Parse(ParseEventHandler, new ListBuilderContext())).Root;
        }

        private static void ParseEventHandler(ParseEventArgs e)
        {
            var pctx = e.ParserContext as ListBuilderContext;
            if (pctx == null)
            {
                return;
            }

            switch (e.Token)
            {
                case Token.CommentStart:
                    return;

                case Token.SExprStart:
                {
                    // a new list child
                    pctx.Current = new LispList(pctx.Current as LispList);

                    if (pctx.Root == null)
                    {
                        pctx.Root = pctx.Current;
                    }
                }
                    return;

                case Token.SExprFinish:
                {
                    // pop back one level
                    pctx.Current = pctx.Current.Parent;
                }
                    return;
            }

            switch (e.State)
            {
                case ParseState.Atom:
                {
                    // a new atom child
                    var atom = new LispAtom(pctx.Current as LispList, e.TokenValue, e.Token);
                    if (pctx.Current == null)
                    {
                        pctx.Current = atom;
                    }
                    if (pctx.Root == null)
                    {
                        pctx.Root = pctx.Current;
                    }
                }
                    return;

                case ParseState.Failure:
                {
                    throw new ParseException(
                        String.Format("*** FAILURE {0}] : [ {1} ], {2}, {3}", e.ErrorDescription, e.TokenValue, e.State, e.Token));
                }
            }
        }
    }

    internal class ListBuilderContext
    {
        public ListBuilderContext(ILispNode root)
        {
            Root = root;
            Current = root;
        }

        public ListBuilderContext() : this(null) {}
        public ILispNode Root { get; set; }
        public ILispNode Current { get; set; }
    }
}