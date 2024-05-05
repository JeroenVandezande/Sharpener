using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class MethodElement: SyntaxElement, ISyntaxElementWithScope, ISyntaxElementAutoReturnsFromScope, IGenerateMemberSyntax
{
    private bool _ParamType;
    private String _ParamName;
    private bool _NextParamIsReturnType;
    public string MethodName { get; set; }
    public string ReturnType { get; set; } = "void";
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }
    
    public MethodElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        visibilityLevel = visibilityLevel;
        return this;
    }

    public MethodElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (tokenType == TokenType.ClosedParathesis)
        {
            _NextParamIsReturnType = true;
            return;
        }

        if (_NextParamIsReturnType)
        {
            ReturnType = param;
            return;
        }
        
        if (string.IsNullOrEmpty(MethodName))
        {
            MethodName = param;
        }
        else
        {
            if (!_ParamType)
            {
                _ParamName = param;
                _ParamType = true;
            }
            else
            {
                var kvp = new KeyValuePair<string, string>(_ParamName, param);
                Parameters.Add(kvp);
                _ParamType = false;
            }
        }
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
        // Create a method
        var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(ReturnType), MethodName)
            .AddModifiers(SyntaxFactory.Token(vis))
            .WithBody(SyntaxFactory.Block());
        if (IsStatic)
        {
            methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        foreach (var child in Children)
        {
            if (child is IGenerateStatementSyntax)
            {
                var ch = (IGenerateStatementSyntax) child;
                methodDeclaration = methodDeclaration.AddBodyStatements(ch.GenerateCodeNode());
            }
        }

        return methodDeclaration;
    }
}