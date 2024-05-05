using System;
using System.Collections.Generic;
using System.Linq;

namespace TestLibrary
{
    public class SimpleClass
    {
        public String SimpleString { get; set; }

        public void InternalSimpleMethod()
        {
        }
    }

    public class TestClassName
    {
        private Int32 _staticPrivate { get; set; }

        private  9.9  _testDoubleLiteral { get; set; }

        private  9  _testIntLiteral { get; set; }

        protected String protectedString { get; set; }

        public void EmptyMethod()
        {
        }

        Double TestDoubleLiteral { get; set; }
    }
}