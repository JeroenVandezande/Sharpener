using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener.SyntaxTree.SyntaxElements
{
    public class TypeInheritanceSectionElement : SyntaxElement
    {
        public string VariableName { get; set; }

        public override void AddParameter(string param, TokenType tokenType)
        {
            if (String.IsNullOrEmpty(VariableName))
            {
                VariableName = param;
            }
        }
    }
}
