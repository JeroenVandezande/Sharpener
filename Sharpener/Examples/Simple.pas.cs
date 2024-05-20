using System;
using System.Linq;

namespace SimpleConsoleApp
{
    [AttributeHere, SecondAttribute]
    public class Program
    {
        public static void Main()
        {
            begin // add your own code here
            writeLn('The magic happens here.');
            end; * /
        }
    }
}