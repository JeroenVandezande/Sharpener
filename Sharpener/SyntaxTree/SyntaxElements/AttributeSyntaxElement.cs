using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class SimpleAttributeSyntaxElement : SyntaxElement, ISyntaxElementWithScope
{
    public String AttributeText { get; set; }
    
}

public class AttributeSyntaxElement : SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public String AttributeText { get; set; }
    public MemberDeclarationSyntax GenerateCodeNode()
    {
        throw new NotImplementedException();
    }
}