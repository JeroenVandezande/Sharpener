using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sharpener.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.SyntaxTree.Scopes;

namespace Sharpener.SyntaxTree;

public interface ISyntaxElement
{
    public List<ISyntaxElement> Children { get; set; }
    public ISyntaxElement Parent { get; set; }
    public string OriginalSourceCode { get; set; }
    public int OriginalSourceCodeStartLineNumber { get; set; }
    public int OriginalSourceCodeStartColumnNumber { get; set; }
    public int OriginalSourceCodeStopLineNumber { get; set; }
    public int OriginalSourceCodeStopColumnNumber { get; set; }
    public bool ElementIsFinished { get; set; }
    /// <summary>
    /// Gives a token to the current Element
    /// </summary>
    /// <param name="document">The Document object that keeps track of things</param>
    /// <param name="token">The Token that is given to the Element</param>
    /// <returns>true = token is handled by Element. false = token needs to be further parsed.</returns>
    public bool WithToken(Document document, IToken token);
    public void FinishSyntaxElement(Document document);
    
}

public interface IGenerateMemberSyntax : ISyntaxElement
{
    public List<MemberDeclarationSyntax> GenerateCodeNodes();
}

public interface IGenerateCodeBlock : ISyntaxElement
{
    public List<BlockSyntax> GenerateCodeNodes();
}

public interface IGenerateExpressionSyntax : ISyntaxElement
{
    public List<ExpressionSyntax> GenerateCode();
}

public interface IGenerateStatementSyntax : ISyntaxElement
{
    public List<StatementSyntax> GenerateCodeNodes();
}

public interface ISyntaxElementWithScope
{
}

public interface IAttributeElement
{
    public bool AttributeRequiresCodeConversion { get; set; }
    public String AttributeText { get; set; }
    public List<AttributeSyntax>  GenerateCodeNodes();
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
    [JsonIgnore] 
    [IgnoreDataMember]
    public List<IAttributeElement> Attributes { get; set; }

    public string OriginalSourceCode { get; set; }
    public int OriginalSourceCodeStartLineNumber { get; set; }
    public int OriginalSourceCodeStartColumnNumber { get; set; }
    public int OriginalSourceCodeStopLineNumber { get; set; }
    public int OriginalSourceCodeStopColumnNumber { get; set; }
    public bool ElementIsFinished { get; set; }
    public virtual bool WithToken(Document document, IToken token)
    {
        return false;
    }

    public virtual void FinishSyntaxElement(Document document)
    {
        var subset = document.OriginalOxygeneCode.Skip(OriginalSourceCodeStartLineNumber - 1).Take(OriginalSourceCodeStopLineNumber - OriginalSourceCodeStartLineNumber + 1);
        OriginalSourceCode = String.Join(Environment.NewLine, subset);
    }

    /*public virtual void SemicolonWasDetected()
    {
    }*/

    public SyntaxElement WithStartSourceCodePosition(int startLineNumber, int startColumnNumber)
    {
        OriginalSourceCodeStartLineNumber = startLineNumber;
        OriginalSourceCodeStartColumnNumber = startColumnNumber;
        return this;
    }
}

public class NameSpaceElement : SyntaxElement, ISyntaxElementWithScope
{
    public List<String> Usings { get; set; } = new List<string>(){"System"};
    public String NameSpace { get; set; }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (document.IsNotifyKeywordUsedInFile)
        {
            if (!Usings.Contains("PropertyChanged"))
            {
                Usings.Add("PropertyChanged");
            }
        }
        
        if (token.TokenType == TokenType.TypeDeclarationKeyword)
        {
            document.IsInTypePartOfFile = true;
            ElementIsFinished = true;
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            if (String.IsNullOrEmpty(NameSpace))
            {
                NameSpace = param.TokenText;
                return true;
            }
            else
            {
                if (ElementIsFinished)
                {
                    return false;
                }

                if (!Usings.Contains(param.TokenText))
                {
                    Usings.Add(param.TokenText);
                    return true;
                }
            }
        }

        return false;
    }
}
