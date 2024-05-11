using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class MethodElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax, ISyntaxElementAutoReturnsFromScope
{
    private bool _ParamType;
    private String _ParamName;
    private bool _NextParamIsReturnType;
    public string MethodName { get; set; }
    public string ReturnType { get; set; } = "void";
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }
    public bool IsEmpty { get; set; }

    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
    }

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
        //setup parameters
        var paramList = new List<ParameterSyntax>();
        foreach (var par in Parameters)
        {
            paramList.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(par.Key))
                .WithType(SyntaxFactory.ParseTypeName(par.Value)));
        }
        var methodparams = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(paramList));
        
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
        // Create a method
        var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(ReturnType), MethodName)
            .AddModifiers(SyntaxFactory.Token(vis))
            .WithBody(SyntaxFactory.Block())
            .WithParameterList(methodparams);
        if (IsStatic)
        {
            methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }
        
        if (IsEmpty)
        {
            return methodDeclaration;
        }

        //methodDeclaration = methodDeclaration.WithBody(ch.GenerateCodeNode());

        var cscode = MethodBodyTranslation.TranslateOxygeneToCS(OriginalSourceCode);
        var cscodeLines = cscode.Split("\n").ToList();
        var methodStatements = new List<StatementSyntax>();
        SyntaxTrivia previousComment = SyntaxFactory.Comment("");
        bool hasComment = false;
        foreach (var codeline in cscodeLines)
        {
            if (codeline.Trim().StartsWith("//"))
            {
                previousComment = SyntaxFactory.Comment(codeline.Trim());
                hasComment = true;
            }
            else
            {
                var ms = SyntaxFactory.ParseStatement(codeline);
                if (hasComment)
                {
                    ms.WithLeadingTrivia(previousComment);
                    hasComment = false;
                }
                methodStatements.Add(ms);
            }
        }
        methodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block(methodStatements));
        return methodDeclaration;
    }
}