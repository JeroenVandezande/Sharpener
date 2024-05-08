using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sharpener.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpener.SyntaxTree;

public interface ISyntaxElement
{
    public List<ISyntaxElement> Children { get; set; }
    public ISyntaxElement Parent { get; set; }
    public void AddParameter(string param, TokenType tokenType);
}

public interface IGenerateMemberSyntax : ISyntaxElement
{
    public MemberDeclarationSyntax GenerateCodeNode();
}

public interface IGenerateCodeBlock : ISyntaxElement
{
    public BlockSyntax GenerateCodeNode();
}

public interface IGenerateExpressionSyntax : ISyntaxElement
{
    public ExpressionSyntax GenerateCode();
}

public interface IGenerateStatementSyntax : ISyntaxElement
{
    public StatementSyntax GenerateCodeNode();
}

public interface ISyntaxElementWithScope
{
}

public interface ISyntaxElementAutoReturnsFromScope
{
}

public interface ISyntaxElementNotInTree
{
}

public abstract class SyntaxElement: ISyntaxElement
{
    public List<ISyntaxElement> Children { get; set; } = new List<ISyntaxElement>();
    [JsonIgnore] 
    [IgnoreDataMember] 
    public ISyntaxElement Parent { get; set; }

    public virtual void AddParameter(string param, TokenType tokenType)
    {
    }
}

public class NameSpaceElement : SyntaxElement, ISyntaxElementWithScope
{
    public List<String> Usings { get; set; } = new List<string>(){"System"};
    public String NameSpace { get; set; }
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(NameSpace))
        {
            NameSpace = param;
        }
        else
        {
            if (!Usings.Contains(param))
            {
                Usings.Add(param);
            }
        }
    }
}
