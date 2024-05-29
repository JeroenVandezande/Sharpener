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

        public const string STRING_CONST = "MyConstantString";
        public const byte INT_CONST = 80;
        public const float DOUBLE_CONST = 155.0;
        public const float e = 1e-6;
        public static readonly Integer EXPLICITINT_CONST = Integer65536;
        public static readonly System.Drawing.Color.FromArgb ( 0 ,  0 ,  50 ,  100 )EXPLICITOBJECT_CONST = System.Drawing.Color.FromArgb(0, 0, 50, 100);
    }
}