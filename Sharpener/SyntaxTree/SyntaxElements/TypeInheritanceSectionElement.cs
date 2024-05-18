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

        public override bool WithToken(Document document, IToken token)
        {
            if (token is ITokenWithText param)
            {
                if (String.IsNullOrEmpty(VariableName))
                {
                    VariableName = param.TokenText;
                    return true;
                }
            }

            return false;
        }
    }
}
