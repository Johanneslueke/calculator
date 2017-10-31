using System;
using System.Collections.Generic;
using System.Globalization;

namespace BrightSword.LightSaber.Module
{
    public static class LispDate
    {
        [FunctionName("today")]
        public static ILispNode Today(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var formatParam = (arguments.Count > 0)
                    ? arguments[0].Eval(callStack, true)
                    : null;
                var strFormat = (((formatParam == null) || (formatParam is LispNil))
                    ? String.Empty
                    : (formatParam as LispAtom).ValueAsString);

                return new LispAtom(DateTime.Today.ToString(strFormat));
            }
            catch
            {
                return new LispNil();
            }
        }

        [FunctionName("now")]
        public static ILispNode Now(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var formatParam = (arguments.Count > 0)
                    ? arguments[0].Eval(callStack, true)
                    : null;
                var strFormat = (((formatParam == null) || (formatParam is LispNil))
                    ? String.Empty
                    : (formatParam as LispAtom).ValueAsString);

                return new LispAtom(DateTime.Now.ToString(strFormat));
            }
            catch
            {
                return new LispNil();
            }
        }

        [FunctionName("date")]
        public static ILispNode Date(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            DateTime result;
            if (DateTime.TryParse((arguments[0].Eval(callStack, true) as LispAtom).ValueAsDateTime.ToString(), out result))
            {
                var formatParam = (arguments.Count > 1)
                    ? arguments[1].Eval(callStack, true)
                    : null;
                var strFormat = ((formatParam is LispNil)
                    ? String.Empty
                    : (formatParam as LispAtom).ValueAsString);

                return new LispAtom(result.ToString(strFormat));
            }
            else
            {
                return new LispNil();
            }
        }

        [FunctionName("date?")]
        [FunctionName("date-p")]
        [FunctionName("datetime?")]
        [FunctionName("datetime-p")]
        public static ILispNode DateP(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            DateTime result;
            return new LispAtom(DateTime.TryParse((arguments[0].Eval(callStack, true) as LispAtom).ValueAsDateTime.ToString(), out result));
        }

        [FunctionName("day_of_week")]
        public static ILispNode DayOfWeek(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            DateTime result;
            if (DateTime.TryParse((arguments[0].Eval(callStack, true) as LispAtom).ValueAsDateTime.ToString(), out result))
            {
                return new LispAtom((int) result.DayOfWeek);
            }
            else
            {
                return new LispNil();
            }
        }

        [FunctionName("day_of_year")]
        public static ILispNode DayOfYear(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            DateTime result;
            if (DateTime.TryParse((arguments[0].Eval(callStack, true) as LispAtom).ValueAsDateTime.ToString(), out result))
            {
                return new LispAtom(result.DayOfYear);
            }
            else
            {
                return new LispNil();
            }
        }

        [FunctionName("week_of_year")]
        public static ILispNode WeekOfYear(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            DateTime result;
            if (DateTime.TryParse((arguments[0].Eval(callStack, true) as LispAtom).ValueAsDateTime.ToString(), out result))
            {
                return new LispAtom(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(result, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Sunday));
            }
            else
            {
                return new LispNil();
            }
        }

#if false
        internal static void Init()
        {
            "(defun yesterday (f?) (date (- (today) 1) f))".ParseAndEvaluate(null);
            "(defun tomorrow (f?) (date (+ (today) 1) f))".ParseAndEvaluate(null);
            "(defun day_name (x) (date x 'dddd'))".ParseAndEvaluate(null);
            "(defun day_of_month (x) (date x 'd'))".ParseAndEvaluate(null);
            "(defun month (x) (date x 'M'))".ParseAndEvaluate(null);
            "(defun short_month (x) (date x 'MMM'))".ParseAndEvaluate(null);
            "(defun long_month (x) (date x 'MMMM'))".ParseAndEvaluate(null);
            "(defun short_year (x) (date x 'YY'))".ParseAndEvaluate(null);
            "(defun long_year (x) (date x 'YYYY'))".ParseAndEvaluate(null);
            @"(defun next_sunday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_monday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -1 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_tuesday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -2 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_wednesday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -3 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_thursday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -4 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_friday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -5 (day_of_week d))) f)))".ParseAndEvaluate(null);
            @"(defun next_saturday (x? f?) 
(prog
    (setq d (if (nil-p x) today x))
    (date (+ d (- 7 -6 (day_of_week d))) f)))".ParseAndEvaluate(null);
        }

        internal static Value ApplyFunction(Value Functor, List<Expression> arguments, Dictionary<string, object> callStack)
        {
            Value result = new Value();

            string strFunc = Functor.ValueAsString;
            Functor.UnregisterMissingSymbol(strFunc);

            switch (strFunc.ToLower())
            {
                default:
                    {
                        throw new Exception(strFunc + " is not a date function!");
                    }


        #region NEGATE/SUBTRACT
                case "-":
                case "sub":
                    {
                        if (arguments.Count < 1) { throw new EvaluatorException(Functor, "NEGATE requires at least 1 argument"); }

                        Value xFirst = arguments[0].Evaluate(callStack);
                        result.MergeMissingSymbols(xFirst);

                        if (!xFirst.IsDateTime) { throw new Exception(); }

                        return result.SetValue(arguments.Skip(1).Aggregate(xFirst.ValueAsDateTime, (result, x) =>
                        {
                            Value xEval = x.Evaluate(callStack);

                            TimeSpan ts;
                            if (!TimeSpan.TryParse(xEval.ValueAsString, out ts))
                            {
                                throw new EvaluatorException(Functor, "SUB (date) requires the first argument to be a date and all subsequent arguments to be timespans");
                            }

                            result.MergeMissingSymbols(xEval);

                            result -= ts;

                            return result;
                        }));
                    }
        #endregion
            }
        }
#endif
    }
}