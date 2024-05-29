using System;
using System.Linq;

namespace SimpleConsoleApp
{
    [AttributeHere, SecondAttribute]
    public class Program
    {
        public TWorklist WorkList { get =>; set =>; }

        IWorklist ITest.Worklist { get; set; }

        public IWorklist UsedWorklist
        {
            get
            {
                return (ITest)this.Worklist;
            }

            set
            {
                (ITest)this.Worklist = value;
            }
        }

        public static Int32 Main()
        {
        }
    }
}