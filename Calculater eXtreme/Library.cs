using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BrightSword.LightSaber
{
    public static class Library
    {
        private static readonly FunctionLibrary _functionLibrary = new FunctionLibrary();

        public static List<ApplyFunc> GetRegisteredFunctionsByName(string strFunctionName)
        {
            return _functionLibrary[strFunctionName];
        }

        public static string Register(string key, ApplyFunc func)
        {
            _functionLibrary[key].Add(func);
            return key;
        }

        public static string Unregister(string key, ApplyFunc func)
        {
            _functionLibrary[key].Remove(func);
            return key;
        }

        public static string Clear(string key)
        {
            _functionLibrary[key].Clear();
            return key;
        }

        public static void Clear()
        {
            _functionLibrary.Clear();
        }

        public static event LoadCompleteEventHandler LoadCompleteEvent;

        private static void OnLoadCompleteEvent(LoadCompleteEventArgs e)
        {
            if (LoadCompleteEvent != null)
            {
                LoadCompleteEvent(null, e);
            }
        }

        public static void Load(Type moduleClass)
        {
            if (!(moduleClass.IsClass))
            {
                throw new LispException(null, "Library::Load requires a Class type");
            }

            foreach (var mi in moduleClass.GetMethods())
            {
                try
                {
                    foreach (FunctionNameAttribute fna in mi.GetCustomAttributes(typeof (FunctionNameAttribute), false))
                    {
                        Register(fna.Name, (ApplyFunc) (Delegate.CreateDelegate(typeof (ApplyFunc), mi)));
                        OnLoadCompleteEvent(new LoadCompleteEventArgs(mi));
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        #region Nested type: FunctionLibrary

        private sealed class FunctionLibrary : Dictionary<string, List<ApplyFunc>>
        {
            public new List<ApplyFunc> this[string key]
            {
                get
                {
                    key = key.ToLower();

                    if (!(Keys.Contains(key)))
                    {
                        base[key] = new List<ApplyFunc>();
                    }

                    Debug.Assert(base[key] != null);

                    return base[key];
                }
            }
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class FunctionNameAttribute : Attribute
    {
        public FunctionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public delegate void LoadCompleteEventHandler(object sender, LoadCompleteEventArgs e);

    public class LoadCompleteEventArgs : EventArgs
    {
        public LoadCompleteEventArgs(MethodInfo method)
        {
            Method = method;
        }

        public MethodInfo Method { get; private set; }
    }
}