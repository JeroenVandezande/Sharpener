using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class SimpleAttributeSyntaxElement : SyntaxElement, IGenerateMemberSyntax
{
    public String AttributeText { get; set; }
    public MemberDeclarationSyntax GenerateCodeNode()
    {
        throw new NotImplementedException();
    }
}

public class AttributeSyntaxElement : SyntaxElement, IGenerateMemberSyntax
{
    public String AttributeText { get; set; }
    public MemberDeclarationSyntax GenerateCodeNode()
    {
        throw new NotImplementedException();
    }
}