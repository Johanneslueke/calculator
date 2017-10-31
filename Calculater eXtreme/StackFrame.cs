using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightSword.LightSaber
{
    public class StackFrame : Dictionary<string, object> {}

    public class CallStack
    {
        private readonly Stack<StackFrame> _callstack = new Stack<StackFrame>();

        public CallStack()
        {
            PushFrame();
        }

        public object this[string key]
        {
            get
            {
                var frame = _callstack.FirstOrDefault(_frame => _frame.ContainsKey(key));
                return frame == null ? null : frame[key];
            }

            set { CurrentFrame[key] = value; }
        }

        public StackFrame CurrentFrame
        {
            get { return _callstack.Peek(); }
        }

        public int NumberOfFrames
        {
            get { return _callstack.Count; }
        }

        public StackFrame PushFrame()
        {
            try
            {
                _callstack.Push(new StackFrame());
                return CurrentFrame;
            }
            catch
            {
                return null;
            }
        }

        public StackFrame PopFrame()
        {
            return _callstack.Pop();
        }

        public override string ToString()
        {
            return _callstack.Aggregate(
                String.Empty, (r, x) => r + (x.Aggregate(String.Empty, (r0, x0) => r0 + (x0.Key + " : " + x0.Value.ToString() + ", ")) + "\n"));
        }
    }
}