using System;
using System.Linq;

namespace Sharperer
{
    public static class RTL
    {
        public static void writeLn(String text)
        {
            Console.WriteLine(text);
        }

        public static bool assigned(Object expression)
        {
            return (expression != null);
        }
        
        public static T coalesce<T>(params T[] list)
        {
            foreach (var v in list)
            {
                if (v != null)
                {
                    return v;
                }
            }

            return list.Last();
        }

        public static char chr(int value)
        {
            return (char) value;
        }

        public static void disposeAndNil(ref Object someObject)
        {
            if (someObject != null && someObject is IDisposable)
            {
               ((IDisposable)someObject).Dispose();
            }
            someObject = null;
        }

    }
}