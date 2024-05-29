using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElementBase: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    protected enum SpecialSyntaxToken
    {
        None,
        Getter,
        Setter,
        Implements
    }
    protected SpecialSyntaxToken SpecialTokenState;
    public override bool WithToken(Document document, IToken token)
    {
        return base.WithToken(document, token);
    }
    public virtual List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        return result;
    }
}