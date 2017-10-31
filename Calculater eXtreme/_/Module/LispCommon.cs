using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber.Module
{
    public static class LispCommon
    {
        // WARNING : DEFINE FUNCTIONS IN THE ORDER OF PRIORITY - FUNCTIONS DEFINED FIRST OVERRIDE FUNCTIONS DEFINED LATER

        [FunctionName("+")]
        [FunctionName("later_by")]
        public static ILispNode AddDate(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                var merger = new AtomMerger(new LispMissing(), (r, x) => (DateTime) r + (TimeSpan) x);

                var result = functor.MergeAsDateTime(arguments, callStack, 2, merger);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("add_date throws: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(add_date {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("+")]
        [FunctionName("add")]
        [FunctionName("sum")]
        public static ILispNode AddNumber(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                var merger = new AtomMerger(new LispMissing(), (r, x) => (double) r + (double) x);

                var result = functor.MergeAsNumber(arguments, callStack, 2, merger);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("add_number throws: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(add_number {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("+")]
        [FunctionName("cat")]
        [FunctionName("concat")]
        public static ILispNode Concat(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                var merger = new AtomMerger(new LispMissing(), (r, x) => (((string) r).Unquote() + ((string) x).Unquote()).Quote());

                var result = functor.MergeAsString(arguments, callStack, 2, merger);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("concat throws: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(concat {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("-")]
        [FunctionName("earlier_by")]
        public static ILispNode SubDate(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                var merger = new AtomMerger(new LispMissing(), (r, x) => (DateTime) r - (TimeSpan) x);

                var result = functor.MergeAsDateTime(arguments, callStack, 2, merger);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("sub_date throws: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(sub_date {0}) done", arguments.ToLispArgString());
            }
#endif
        }

        [FunctionName("-")]
        [FunctionName("sub")]
        public static ILispNode SubNumber(ILispNode functor, IList<ILispNode> arguments, CallStack callStack, params object [ ] args)
        {
#if TRACE_FLOW
            try
            {
#endif
                var merger = new AtomMerger(new LispMissing(), (r, x) => (double) r - (double) x);

                var result = functor.MergeAsNumber(arguments, callStack, 2, merger);

                return (merger.MissingSymbols.Count() > 0)
                    ? merger.MissingSymbols
                    : result;
#if TRACE_FLOW
            }
            catch (Exception ex)
            {
                Console.WriteLine("sub_number throws: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("(sub_number {0}) done", arguments.ToLispArgString());
            }
#endif
        }
    }
}