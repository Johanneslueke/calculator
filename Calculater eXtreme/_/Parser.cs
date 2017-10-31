//-----------------------------------------------------------------------------
// (c) 2008 BrightSword Technologies Private Limited. All rights reserved.
//                                                                             
// Author : John S. Azariah (johnaz@brightsword.com)                           
// Date   : 2008 08 03                                                        
//                                                                             
//-----------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace BrightSword.LightSaber
{
    public static class Parser
    {
        /// <summary>
        /// Parses a string representation of an s-expression
        /// 
        /// Expression ::= Atom | SExpr
        /// Atom ::= ValidName | Number | QuotedString
        /// SExpr ::= "(" Atom | SExpr ")"
        /// 
        /// Number ::= ^[-+]?[0-9]*\\.?[0-9]+([eE][-+]?[0-9]*\\.?[0-9]+)?$
        /// ValidName ::= ^[a-zA-Z_]([a-zA-Z0-9_])*$
        /// QuotedString ::= single or doubly quoted text of arbitrary length, with or without whitespace
        /// 
        /// </summary>
        /// <param name="psz">Input string to be parsed</param>
        /// <param name="parseEventHandler">Function to be notified of Parse events</param>
        /// <param name="context">Any object to be passed along to the notification sink</param>
        /// <returns>The context object if successful, null if not</returns>
        ///
        /// <example>
        /// 
        /// Given the following support objects
        ///
        ///internal class EvaluatorContext
        ///{
        ///    public Expression Root { get; private set; }
        ///    public Expression Current { get; set; }
        ///
        ///    public EvaluatorContext(Expression Root)
        ///    {
        ///        this.Root = Root;
        ///        this.Current = Root;
        ///    }
        ///}
        ///
        ///protected static void ParseEventHandler(ParseEventArgs e)
        ///{
        ///    EvaluatorContext pctx = e.ParserContext as EvaluatorContext;
        ///    if (pctx == null) return;
        ///
        ///    switch (e.Token)
        ///    {
        ///        case E_TOKEN.Comment:
        ///            return;
        ///
        ///        case E_TOKEN.SExprStart:
        ///            {
        ///                /// a new list child
        ///                pctx.Current = new Expression(pctx.Current, "CHILD >", e.State, e.Token);
        ///            }
        ///            return;
        ///
        ///        case E_TOKEN.SExprFinish:
        ///            {
        ///                /// pop back one level
        ///                pctx.Current = pctx.Current.Parent;
        ///            }
        ///            return;
        ///    }
        ///
        ///    switch (e.State)
        ///    {
        ///        case E_PARSESTATE.Atom:
        ///            {
        ///                /// a new atom child
        ///                Expression Atom = new Expression(pctx.Current, e.TokenValue, e.State, e.Token);
        ///            }
        ///            return;
        ///
        ///        case E_PARSESTATE.Failure:
        ///            {
        ///                throw new ParseException(String.Format("*** FAILURE {0}] : [ {1} ], {2}, {3}", e.ErrorDescription, e.TokenValue, e.State, e.Token));
        ///            }
        ///    }
        ///}
        ///
        ///we can write ...
        /// 
        ///EvaluatorContext Context = "(+ 'Hello' ' ' 'World ')".Parse(ParseEventHandler, new EvaluatorContext(new Expression()));
        ///Expression exHelloWorld = Context.Root.Evaluate(null);
        ///
        ///System.Console.WriteLine
        ///
        /// </example>
        /// 
        public static object Parse(this string psz, ParseEventHandler parseEventHandler, object context)
        {
            var sStart = 0;
            var sFinish = 0;
            var exprNest = 0;

            var state = ParseState.Start;

            while (sFinish <= psz.Length)
            {
                switch (state)
                {
                    case ParseState.Start:
                    {
                        exprNest = 0;

                        if (parseEventHandler != null)
                        {
                            parseEventHandler(new ParseEventArgs(psz, context, state, Token.MAX, 0, 0));
                        }

                        state = ParseState.AtomOrSExpr;
                    }
                        break;

                    case ParseState.Finish:
                    {
                        if (exprNest != 0)
                        {
                            state = ParseState.Failure;
                            break;
                        }

                        if (parseEventHandler != null)
                        {
                            parseEventHandler(new ParseEventArgs(psz, context, state, Token.MAX, 0, 0));
                        }

                        // we're done...
                        return context;
                    }
                }

                var tok = Tokenizer.NextToken(psz, sFinish, out sStart, out sFinish);

                // handle comments and get them out of the way
                switch (tok)
                {
                    case Token.CommentStart: // /*
                    {
                        Tokenizer.SnarfComment(psz, sStart, out sStart, out sFinish);

                        if (parseEventHandler != null)
                        {
                            parseEventHandler(new ParseEventArgs(psz, context, ParseState.Comment, Token.CommentStart, sStart, sFinish));
                        }
                    }
                        continue;
                }

                switch (state)
                {
                    case ParseState.SExprStart:
                    {
                        // SExpr ::= SExprStart (Atom | SExpr)* SExprFinish
                        switch (tok)
                        {
                            case Token.SExprStart: // (
                            {
                                exprNest++;

                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(new ParseEventArgs(psz, context, ParseState.SExprStart, tok, sStart, sFinish));
                                }

                                state = ParseState.AtomOrSExpr;
                            }
                                break;

                            case Token.EOF:
                            {
                                sFinish = sStart;
                                state = ParseState.Finish;
                            }
                                break;

                            default:
                            {
                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(
                                        new ParseEventArgs(
                                            psz,
                                            context,
                                            ParseState.SExprStart,
                                            tok,
                                            sStart,
                                            sFinish,
                                            "Unexpected token found instead of SExprStart"));
                                }

                                state = ParseState.Failure;
                            }
                                break;
                        }
                    }
                        break;

                    case ParseState.AtomOrSExpr:
                    {
                        switch (tok)
                        {
                            case Token.DoubleQuotedText:
                            case Token.SingleQuotedText:
                            {
                                if (parseEventHandler != null)
                                {
                                    var strToken = psz.Substring(sStart, sFinish - sStart);
                                    var fContainsWhitespace = Regex.IsMatch(strToken, ".*[\\s]+.*");
                                    var cSkip = (fContainsWhitespace
                                        ? 0
                                        : 1);

                                    parseEventHandler(new ParseEventArgs(psz, context, ParseState.Atom, tok, sStart + cSkip, sFinish - cSkip));
                                }
                                state = ParseState.AtomOrSExpr;
                            }
                                break;

                            case Token.ValidName:
                            case Token.ValidNumber:
                            case Token.Text:
                            {
                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(new ParseEventArgs(psz, context, ParseState.Atom, tok, sStart, sFinish));
                                }
                                state = ParseState.AtomOrSExpr;
                            }
                                break;

                            case Token.SExprStart: // (
                            {
                                sFinish = sStart; //rewind token
                                state = ParseState.SExprStart;
                            }
                                break;

                            case Token.SExprFinish: // )
                            {
                                sFinish = sStart; //rewind token
                                state = ParseState.SExprFinish;
                            }
                                break;

                            case Token.EOF:
                            {
                                state = ParseState.Finish;
                            }
                                break;

                            default:
                            {
                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(
                                        new ParseEventArgs(
                                            psz,
                                            context,
                                            ParseState.Failure,
                                            tok,
                                            sStart,
                                            sFinish,
                                            "Unexpected token found without matching SExprFinish"));
                                }
                                state = ParseState.Failure;
                            }
                                break;
                        }
                    }
                        break;

                    case ParseState.SExprFinish:
                    {
                        switch (tok)
                        {
                            case Token.SExprFinish: // )
                            {
                                exprNest--;

                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(new ParseEventArgs(psz, context, ParseState.SExprFinish, tok, sStart, sFinish));
                                }
                                state = (exprNest == 0)
                                    ? ParseState.Finish
                                    : ParseState.AtomOrSExpr;
                            }
                                break;

                            case Token.EOF:
                            {
                                sFinish = sStart;
                                state = ParseState.Finish;
                            }
                                break;

                            default:
                            {
                                if (parseEventHandler != null)
                                {
                                    parseEventHandler(
                                        new ParseEventArgs(
                                            psz, context, ParseState.Failure, tok, sStart, sFinish, "Unexpected token found instead of SExprFinish"));
                                }
                                state = ParseState.Failure;
                            }
                                break;
                        }
                    }
                        break;

// ReSharper disable RedundantCaseLabel
                    case ParseState.Failure:
// ReSharper restore RedundantCaseLabel
                    default:
                    {
                        if (parseEventHandler != null)
                        {
                            parseEventHandler(new ParseEventArgs(psz, context, ParseState.Failure, Token.MAX, sStart, sFinish, "Parser Failure"));
                        }
                    }

                        return null;
                }
            }
            // while (curpos < psz.Length)

            if (parseEventHandler != null)
            {
                parseEventHandler(new ParseEventArgs(psz, context, ParseState.Failure, Token.MAX, sStart, sFinish));
            }

            return null;
        }

        #region Nested type: Tokenizer

        internal class Tokenizer
        {
            internal static bool IsValidName(string str)
            {
                return Regex.IsMatch(str, Constants.C_REGEXP_VALIDNAME, RegexOptions.IgnorePatternWhitespace);
            }

            internal static bool IsValidNumber(string str)
            {
                return Regex.IsMatch(str, Constants.C_REGEXP_FLOATINGPOINT, RegexOptions.IgnorePatternWhitespace);
            }

            internal static bool IsWhiteSpace(char ch)
            {
                switch (ch)
                {
                    case '\t':
                    case '\r':
                    case '\n':
                    case ' ':
                        return true;
                    default:
                        return false;
                }
            }

            internal static bool IsLetter(char ch)
            {
                int ich = ch;
                return (((0x41 <= ich) && (ich <= 0x5A)) || ((0x61 <= ich) && (ich <= 0x7A)) || ((0xC0 <= ich) && (ich <= 0xD6))
                    || ((0xD8 <= ich) && (ich <= 0xF6)) || ((0xF8 <= ich) && (ich <= 0xFF)));
            }

            internal static bool IsDigit(char ch)
            {
                int ich = ch;
                return ((0x30 <= ich) && (ich <= 0x39));
            }

            internal static bool IsOpenParen(char ch)
            {
                return (ch == '(');
            }

            internal static bool IsCloseParen(char ch)
            {
                return (ch == ')');
            }

            internal static bool IsBang(char ch)
            {
                return (ch == '!');
            }

            internal static bool IsLessThan(char ch)
            {
                return (ch == '<');
            }

            internal static bool IsMoreThan(char ch)
            {
                return (ch == '>');
            }

            internal static bool IsEqualSign(char ch)
            {
                return (ch == '=');
            }

            internal static bool IsPlusSign(char ch)
            {
                return (ch == '+');
            }

            internal static bool IsMinusSign(char ch)
            {
                return (ch == '-');
            }

            internal static bool IsMultSign(char ch)
            {
                return IsAsterisk(ch);
            }

            internal static bool IsDivSign(char ch)
            {
                return IsSlash(ch);
            }

            internal static bool IsTilde(char ch)
            {
                return (ch == '~');
            }

            internal static bool IsQuote(char ch)
            {
                return (ch == '\'');
            }

            internal static bool IsDblQuote(char ch)
            {
                return (ch == '"');
            }

            internal static bool IsSlash(char ch)
            {
                return (ch == '/');
            }

            internal static bool IsAsterisk(char ch)
            {
                return (ch == '*');
            }

            internal static bool IsAmpersand(char ch)
            {
                return (ch == '&');
            }

            internal static bool IsPipe(char ch)
            {
                return (ch == '|');
            }

            internal static bool IsHat(char ch)
            {
                return (ch == '^');
            }

            internal static bool IsDash(char ch)
            {
                return (ch == '-');
            }

            internal static bool IsUnderscore(char ch)
            {
                return (ch == '_');
            }

            internal static bool IsDot(char ch)
            {
                return (ch == '.');
            }

            internal static bool IsSemiColon(char ch)
            {
                return (ch == ';');
            }

            internal static bool IsHash(char ch)
            {
                return (ch == '#');
            }

            internal static bool IsQuery(char ch)
            {
                return (ch == '?');
            }

            internal static bool IsOpenSqBkt(char ch)
            {
                return (ch == '[');
            }

            internal static bool IsCloseSqBkt(char ch)
            {
                return (ch == ']');
            }

            internal static bool IsColon(char ch)
            {
                return (ch == ':');
            }

            internal static bool SnarfComment(string inputString, int s, out int ps, out int pf)
            {
                return SnarfBlock(inputString, s, out ps, out pf, Token.CommentStart, Token.CommentFinish);
            }

            internal static int SnarfWhiteSpace(string inputString, int s)
            {
                var psz = inputString;

                for (; s < psz.Length && IsWhiteSpace(psz[s]); s++) {}
                return s;
            }

            internal static bool SnarfBlock(string inputString, int s, out int ps, out int pf, Token block_open, Token block_close)
            {
                var tok = NextToken(inputString, s, out ps, out pf);
                if (tok != block_open)
                {
                    return false;
                }

                var t = ps;

                for (; tok != block_close; tok = NextToken(inputString, pf, out ps, out pf))
                {
                    if (tok == Token.EOF)
                    {
                        return false;
                    }
                }

                ps = t;

                return true;
            }

            internal static bool SnarfQuotedText(string psz, int sb, out int sTokBegin, out int sTokEnd, char quote)
            {
                sTokBegin = sb;
                sTokEnd = sb;
                if (psz[sb] != quote)
                {
                    return false;
                }

                sb++;

                while ((sb < (psz.Length - 1)) && (psz[sb] != quote) && (psz[sb - 1] != '\\'))
                {
                    sb++;
                }

                if (psz[sb] != quote)
                {
                    // cannot find the matching quote
                    sTokEnd = sb;
                    return false;
                }

                // step over the quote..
                sb++;

                sTokEnd = sb;
                return true;
            }

            internal static bool SnarfSExpr(string inputString, int s, out int ps, out int pf)
            {
                var tok = NextToken(inputString, s, out ps, out pf);
                if (tok != Token.SExprStart)
                {
                    return false;
                }

                var t = ps;
                var nest = 1;

                while (nest > 0)
                {
                    tok = NextToken(inputString, pf, out ps, out pf);

                    if (tok == Token.EOF)
                    {
                        return false;
                    }

                    if (tok == Token.SExprStart)
                    {
                        nest++;
                    }
                    if (tok == Token.SExprFinish)
                    {
                        --nest;
                    }
                }

                ps = t;

                return true;
            }

            internal static bool ValidateName(string inputString, int s, out int ps, out int pf)
            {
                pf = inputString.Length - 1;

                // Name          (Letter | UnderscoreOrColon) (NameChar)*
                // NameChar      Letter | Digit | Dot | Dash | UnderscoreOrColon      
                var psz = inputString;

                ps = SnarfWhiteSpace(inputString, s);
                s = ps;

                if (!(IsLetter(psz[s]) || IsUnderscore(psz[s])))
                {
                    return false;
                }

                ++s;

                for (var fOK = true; (s < psz.Length) && fOK; s++)
                {
                    var ch = psz[s];
                    fOK = (IsLetter(ch) || IsUnderscore(psz[s]) || IsDigit(ch) || IsDash(ch));
                }
                pf = --s;
                return (s < psz.Length);
            }

            public static Token NextToken(string psz, int s, out int sTokBegin, out int sTokEnd)
            {
                sTokBegin = -1;
                sTokEnd = psz.Length - 1;
                if (s < 0)
                {
                    return Token.EOF;
                }

                var sb = SnarfWhiteSpace(psz, s);

                sTokBegin = sb;
                if (sb == psz.Length)
                {
                    return Token.EOF;
                }

                // (
                if (IsOpenParen(psz[sb]))
                {
                    sTokEnd = ++sb;
                    return Token.SExprStart;
                }

                // )
                if (IsCloseParen(psz[sb]))
                {
                    sTokEnd = ++sb;
                    return Token.SExprFinish;
                }

                // /*
                if (IsSlash(psz[sb]) && IsAsterisk(psz[sb + 1]))
                {
                    sb++;
                    sTokEnd = ++sb;
                    return Token.CommentStart;
                }

                // */
                if (IsAsterisk(psz[sb]) && IsSlash(psz[sb + 1]))
                {
                    sb++;
                    sTokEnd = ++sb;
                    return Token.CommentFinish;
                }

                // '...'
                var se = sb;
                if (psz[sb] == '\'')
                {
                    return SnarfQuotedText(psz, sb, out sTokBegin, out sTokEnd, psz[sb])
                        ? Token.SingleQuotedText
                        : Token.EOF;
                }
                sb = se;

                // "..."
                se = sb;
                if (psz[sb] == '"')
                {
                    return SnarfQuotedText(psz, sb, out sTokBegin, out sTokEnd, psz[sb])
                        ? Token.DoubleQuotedText
                        : Token.EOF;
                }
                sb = se;

                {
                    sb++;

                    var fOK = true;
                    for (; (sb < psz.Length) && fOK; sb++)
                    {
                        var ch = psz[sb];
                        fOK = (!(IsWhiteSpace(ch) || IsOpenParen(ch) || IsCloseParen(ch)));
                    }

                    if (!fOK)
                    {
                        --sb;
                    }

                    if (sb <= psz.Length)
                    {
                        sTokEnd = sb;
                        sTokBegin = SnarfWhiteSpace(psz, sTokBegin);

                        var strToken = psz.Substring(sTokBegin, sTokEnd - sTokBegin);

                        if (IsValidName(strToken))
                        {
                            return Token.ValidName;
                        }
                        if (IsValidNumber(strToken))
                        {
                            return Token.ValidNumber;
                        }

                        return Token.Text;
                    }

                    sTokEnd = sb;
                    return Token.EOF;
                }
            }
        }

        #endregion
    }

    #region ParseException

    /// <summary>
    /// An instance of this class is thrown by the Parser class to signify an operational failure.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) {}
    }

    #endregion

    /// <summary>
    /// Global Constant Repository
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The regular expression that a floating point conforms to.
        /// </summary>
        public const string C_REGEXP_FLOATINGPOINT = "^[-+]?[0-9]*\\.?[0-9]+([eE][-+]?[0-9]*\\.?[0-9]+)?$";

        /// <summary>
        /// The regular expression that a valid name conforms to.
        /// </summary>
        public const string C_REGEXP_VALIDNAME = "^[a-zA-Z_]([a-zA-Z0-9_])*$";
    }

    /// <summary>
    /// Valid Tokens returned by the Tokenizer 
    /// </summary>
    public enum Token
    {
        EOF,
        SExprStart,
        SExprFinish,
        CommentStart,
        CommentFinish,
        Quote,
        DblQuote,
        ValidName,
        ValidNumber,
        Text,
        SingleQuotedText,
        DoubleQuotedText,
        MAX
    }

    /// <summary>
    /// Valid Parser States
    /// </summary>
    public enum ParseState
    {
        Failure,
        Start,
        Finish,
        Comment,
        SExprStart,
        SExprFinish,
        AtomOrSExpr,
        Atom,
        MAX
    }

    #region ParseEvent

    /// <summary>
    /// An instance of this delegate should be passed into the Parse function to receive callbacks.
    /// </summary>
    /// <param name="e">Information about the event being raised</param>
    public delegate void ParseEventHandler(ParseEventArgs e);

    /// <summary>
    /// Encapsulates information about the Parse event being raised
    /// </summary>
    public class ParseEventArgs
    {
        public ParseEventArgs(
            string inputString,
            object parserContext,
            ParseState state,
            Token token,
            int sStart,
            int sFinish,
            string errorDescription = "",
            int errorCode = 0)
        {
            InputString = inputString;
            ParserContext = parserContext;
            State = state;
            Token = token;
            TokenValue = ((sStart >= 0) && (sFinish > sStart))
                ? inputString.Substring(sStart, sFinish - sStart)
                : "";
            ErrorDescription = errorDescription;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// The input string being parsed.
        /// </summary>
        public string InputString { get; private set; }

        /// <summary>
        /// The current state of the Parser state machine
        /// </summary>
        public ParseState State { get; private set; }

        /// <summary>
        /// The current token returned by the tokenizer
        /// </summary>
        public Token Token { get; private set; }

        /// <summary>
        /// The actual substring represented by the token
        /// </summary>
        public string TokenValue { get; private set; }

        /// <summary>
        /// A description of the error encountered
        /// </summary>
        public string ErrorDescription { get; private set; }

        internal int ErrorCode { get; private set; }

        /// <summary>
        /// The context object. 
        /// This is passed straight through from the caller of the Parse function for the use of the ParseEventHandler.
        /// </summary>
        public object ParserContext { get; private set; }
    }

    #endregion
}