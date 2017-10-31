using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber.Module
{
    public static class LispCompare
    {
        [FunctionName("==")]
        [FunctionName("equal")]
        [FunctionName("all_same")]
        [FunctionName("eq")]
        public static ILispNode Eq(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker("==");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker("==");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker("==");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker("==");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception("== requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName("!=")]
        [FunctionName("not_equal")]
        [FunctionName("neq")]
        public static ILispNode Neq(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            #region DateTime

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker("!=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            #endregion

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker("!=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker("!=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker("!=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception("!= requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName(">")]
        [FunctionName("strictly_descending")]
        [FunctionName("desc_s")]
        [FunctionName("gt")]
        public static ILispNode Gt(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker(">");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker(">");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker(">");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker(">");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception("> requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName(">=")]
        [FunctionName("descending")]
        [FunctionName("desc")]
        [FunctionName("gte")]
        public static ILispNode Gte(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker(">=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker(">=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker(">=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker(">=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception(">= requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName("<")]
        [FunctionName("strictly_ascending")]
        [FunctionName("asc_s")]
        [FunctionName("lt")]
        public static ILispNode Lt(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker("<");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker("<");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker("<");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker("<");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception("< requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName("<=")]
        [FunctionName("ascending")]
        [FunctionName("asc")]
        [FunctionName("lte")]
        public static ILispNode Lte(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker("<=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker("<=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker("<=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker("<=");

                var result = functor.WalkAsBoolean(arguments, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            throw new Exception("<= requires all arguments to be dates, numbers, booleans or strings");
        }

        [FunctionName("xor")]
        [FunctionName("unique")]
        [FunctionName("all_different")]
        public static ILispNode Unique(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
            var Values = arguments.Select(x => x.Eval(callStack, true)).ToList();

            try
            {
                Values.Sort((x, y) => (x as LispAtom).ValueAsDateTime.CompareTo((y as LispAtom).ValueAsDateTime));

                var merger = new AtomMerger(new LispMissing());
                var walker = new DateTimeWalker((r, prev, curr) => {return (((bool) r) && (LispAtom.CastToDateTime(prev) != LispAtom.CastToDateTime(curr)));});

                var result = functor.WalkAsBoolean(Values, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                Values.Sort(
                    (x, y) => ((x as LispAtom).ValueAsNumber < (y as LispAtom).ValueAsNumber)
                        ? -1
                        : +1);

                var merger = new AtomMerger(new LispMissing());
                var walker = new NumberWalker((r, prev, curr) => {return (((bool) r) && (LispAtom.CastToNumber(prev) != LispAtom.CastToNumber(curr)));});

                var result = functor.WalkAsBoolean(Values, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                Values.Sort((x, y) => (String.Compare((x as LispAtom).ValueAsBoolean.ToString(), (y as LispAtom).ValueAsBoolean.ToString(), true)));

                var merger = new AtomMerger(new LispMissing());
                var walker = new BooleanWalker((r, prev, curr) => {return (bool) r && (LispAtom.CastToBoolean(prev) != LispAtom.CastToBoolean(curr));});

                var result = functor.WalkAsBoolean(Values, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            try
            {
                Values.Sort((x, y) => String.Compare((x as LispAtom).ValueAsString, (y as LispAtom).ValueAsString, true));

                var merger = new AtomMerger(new LispMissing());
                var walker = new StringWalker((r, prev, curr) => {return (((bool) r) && (LispAtom.CastToString(prev) != LispAtom.CastToString(curr)));});

                var result = functor.WalkAsBoolean(Values, callStack, 2, merger, walker);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
            }
            catch {}

            return new LispAtom(true);
            //throw new Exception("UNIQUE requires all arguments to be dates, numbers or strings");
        }
    }
}