using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;
using System.Reflection;

namespace Sharpener.SyntaxTree.Scopes;

public class ConstructorSyntaxImplmentationElement : SyntaxElement, ISyntaxElementWithScope, ISyntaxElementAutoReturnsFromScope
{
    private bool _NextParamIsReturnType;
    private bool _ParamType;
    private String _ParamName;
    public string ClassName { get; set; }
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
        var cls = document.FindClassByName(ClassName);
        if (cls != null)
        {
            foreach (var element in cls.Children)
            {
                if (element is ConstructorSyntaxElement constructorElement)
                {
                    if (constructorElement.ClassName.ToLower() == ClassName.ToLower())
                    {
                        constructorElement.OriginalSourceCode = OriginalSourceCode;
                    }
                }
            }
        }
    }

    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (token.TokenType == TokenType.ClosedParathesis)
            {
                _NextParamIsReturnType = true;
                return true;
            }

            if (string.IsNullOrEmpty(ClassName))
            {
                ClassName = param.TokenText;
                return true;
            }
            else
            {
                if (!_ParamType)
                {
                    _ParamName = param.TokenText;
                    _ParamType = true;
                    return true;
                }
                else
                {
                    var kvp = new KeyValuePair<string, string>(_ParamName, param.TokenText);
                    Parameters.Add(kvp);
                    _ParamType = false;
                    return true;
                }
            }
        }

        return false;
    }
}

public class ConstructorSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax, ISyntaxElementAutoReturnsFromScope
{
    private bool _ParamType;
    private String _ParamName;
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }
    public bool IsEmpty { get; set; }
    public string ClassName { get; private set; }
    
    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
    }
    
    public ConstructorSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public ConstructorSyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }

    public ConstructorSyntaxElement WithClassName(string className)
    {
        ClassName = className;
        return this;
    }

    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (token.TokenType == TokenType.EmptyKeyword)
            {
                if (document.CurrentScope is MethodElement me)
                {
                    var current = document.returnFromCurrentScope();
                    current.OriginalSourceCodeStopLineNumber = token.LineNumber;
                    current.OriginalSourceCodeStopColumnNumber = token.TokenIndex;
                    current.FinishSyntaxElement(document);
                    me.IsEmpty = true;
                    return true;
                }
            }

            if (!_ParamType)
            {
                _ParamName = param.TokenText;
                _ParamType = true;
                return true;
            }
            else
            {
                var kvp = new KeyValuePair<string, string>(_ParamName, param.TokenText);
                Parameters.Add(kvp);
                _ParamType = false;
                return true;
            }
        }

        return false;
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
        var constructorParams = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(paramList));

        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
        // Create a method
        var constructorDeclaration = SyntaxFactory.ConstructorDeclaration(ClassName)
            .AddModifiers(SyntaxFactory.Token(vis))
            .WithBody(SyntaxFactory.Block())
            .WithParameterList(constructorParams);
        if (IsStatic)
        {
            constructorDeclaration = constructorDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        if (IsEmpty)
        {
            return constructorDeclaration;
        }

        var cscode = MethodBodyTranslation.TranslateOxygeneToCS(OriginalSourceCode);
        var cscodeLines = cscode.Split("\n").ToList();
        var constructorStatements = new List<StatementSyntax>();
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
                var cs = SyntaxFactory.ParseStatement(codeline);
                if (hasComment)
                {
                    cs = cs.WithLeadingTrivia(previousComment);
                    hasComment = false;
                }
                constructorStatements.Add(cs);
            }
        }
        constructorDeclaration = constructorDeclaration.WithBody(SyntaxFactory.Block(constructorStatements));

        var attributeSyntaxList = new List<AttributeSyntax>();
        if (Attributes != null)
        {
            foreach (var attr in Attributes)
            {
                attributeSyntaxList.Add(attr.GenerateCodeNode());
            }
        }

        var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeSyntaxList));

        if (attributeSyntaxList.Count > 0)
        {
            constructorDeclaration = constructorDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
        }

        return constructorDeclaration;
    }
}