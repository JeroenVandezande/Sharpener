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
    
    public string? PropertyName { get; set; }
    public string? Propertytype { get; set; }
    public bool IsNullable { get; set; }
    public override bool WithToken(Document document, IToken token)
    {
        if (token.TokenType == TokenType.NullableTypeDefinition)
        {
            IsNullable = true;
            return true;
        }

        if (token is ITokenWithText param)
        {
            if (SpecialTokenState == SpecialSyntaxToken.None)
            {
                if (String.IsNullOrEmpty(PropertyName))
                {
                    PropertyName = param.TokenText;
                    return true;
                }

                if (String.IsNullOrEmpty(Propertytype))
                {
                    Propertytype = param.TokenText;
                    return true;
                }
            }
        }

        return base.WithToken(document, token);
    }
    public virtual List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        return result;
    }
}