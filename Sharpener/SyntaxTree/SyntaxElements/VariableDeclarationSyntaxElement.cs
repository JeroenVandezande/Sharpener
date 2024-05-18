using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class VariableDeclarationSyntaxElement: SyntaxElement
{
    public string VariableName { get; set; }
}

public class ClassVariableDeclarationSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public string VariableName { get; set; }
    public string Variabletype { get; set; }
    
    public bool IsNullable { get; set; }
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }

    public ClassVariableDeclarationSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public ClassVariableDeclarationSyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    
    public ClassVariableDeclarationSyntaxElement WithName(string name)
    {
        VariableName = name;
        return this;
    }

    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(VariableName))
        {
            VariableName = param;
            return;
        }
        if (String.IsNullOrEmpty(Variabletype))
        {
            Variabletype = param;
            return;
        }
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        SyntaxKind vis = SyntaxKind.None;
        switch (VisibilityLevel)
        {
            case VisibilityLevel.Public:
                vis = SyntaxKind.PublicKeyword;
                break;
            case VisibilityLevel.Private:
                vis = SyntaxKind.PrivateKeyword;
                break;
            case VisibilityLevel.Protected:
                vis = SyntaxKind.ProtectedKeyword;
                break;
            case VisibilityLevel.Assembly:
                vis = SyntaxKind.AssemblyKeyword;
                break;
        }

        // Create a Property
        var varType = OxygeneTypeTranslator.ConvertOxygeneTypeToCS(Variabletype);
        varType = varType + (IsNullable ? "?" : "");

        var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(varType))
            .AddVariables(SyntaxFactory.VariableDeclarator(VariableName));

        var fieldDeclaration = SyntaxFactory
            .FieldDeclaration(variableDeclaration)
            .AddModifiers(SyntaxFactory.Token(vis));
        
        if (IsStatic)
        {
            fieldDeclaration = fieldDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }
        
        return fieldDeclaration;
    }
}