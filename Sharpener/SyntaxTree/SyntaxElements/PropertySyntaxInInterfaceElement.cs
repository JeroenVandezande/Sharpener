using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxInInterfaceElement : PropertySyntaxElementBase
{
    private bool _HasGetter;
    private bool _HasSetter;

    public override bool WithToken(Document document, IToken token)
    {

        if (base.WithToken(document, token))
        {
            return true;
        }

        if (token.TokenType == TokenType.ReadGetterKeyword)
        {
            _HasGetter = true;
            return true;
        }

        if (token.TokenType == TokenType.WriteSetterKeyword)
        {
            _HasSetter = true;
            return true;
        }

        if (token.TokenType == TokenType.SemiColon)
        {
            SpecialTokenState = SpecialSyntaxToken.None;
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            return true;
        }

        if (token is ITokenWithText param)
        {
            switch (SpecialTokenState)
            {
                case SpecialSyntaxToken.None: // Regular case handles "PropertyName: Type" syntax
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
                    break;
                }
            }
        }

        return false;
    }

    public override List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = base.GenerateCodeNodes();

        // Create a Property
        if (String.IsNullOrEmpty(Propertytype)) return null;
        var propType = OxygeneTypeTranslator.ConvertOxygeneTypeToCS(Propertytype);
        propType = propType + (IsNullable ? "?" : "");

        var propertyDeclaration = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), PropertyName);
        
        if (_HasGetter)
        {
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        if (_HasSetter)
        {
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
        
        return result;
    }
}