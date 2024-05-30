using Sharpener.SyntaxTree.Scopes;
using Sharpener.Enums;
using Sharpener.SyntaxTree.Literals;
using Sharpener.SyntaxTree.SyntaxElements;
using Sharpener.TokenTypes;

namespace Sharpener.SyntaxTree;

public class TokenParser
{
    private IToken? _previousToken;
    private IToken? _twoTokensBack;

    public void ParseTokens(Tokenizer tokenizer, ref Document document)
    {
        foreach (var token in tokenizer.Tokens)
        {
            if (HandleCurrentElement(document, token)) continue;
            if (HandlePreviousElement(document, token)) continue;

            var tokenWithText = token as ITokenWithText;
            HandleTokenTypes(document, token, tokenWithText);
        }
    }

    private bool HandleCurrentElement(Document document, IToken token)
    {
        if (document.CurrentElement != null && document.CurrentElement.WithToken(document, token))
        {
            UpdateTokenHistory(token);
            return true;
        }
        return false;
    }

    private bool HandlePreviousElement(Document document, IToken token)
    {
        if (document.PreviousElement != null && document.PreviousElement.WithToken(document, token))
        {
            UpdateTokenHistory(token);
            return true;
        }
        return false;
    }

    private void UpdateTokenHistory(IToken token)
    {
        _twoTokensBack = _previousToken;
        _previousToken = token;
    }

    private void HandleTokenTypes(Document document, IToken token, ITokenWithText tokenWithText)
    {
        switch (token.TokenType)
        {
            case TokenType.Variable:
                HandleVariableToken(document, tokenWithText);
                break;
            case TokenType.InterfaceKeyword:
                HandleInterfaceKeyword(document);
                break;
            case TokenType.OpenBracket:
                HandleOpenBracket(document, token);
                break;
            case TokenType.ClosedBracket:
                HandleClosedBracket(document);
                break;
            case TokenType.ImplementationSection:
                HandleImplementationSection(document);
                break;
            case TokenType.EventKeyword:
                HandleEventKeyword(document, token);
                break;
            case TokenType.PropertyKeyword:
                HandlePropertyKeyword(document, token);
                break;
            case TokenType.ConstKeyword:
                HandleConstKeyword(document, token);
                break;
            case TokenType.CodeBlockEnd:
                HandleCodeBlockEnd(document, token);
                break;
            case TokenType.CaseKeyword:
                HandleCaseKeyword(document, token);
                break;
            case TokenType.TypeDeclarationKeyword:
                HandleTypeDeclarationKeyword(document, token);
                break;
            case TokenType.CodeBlockBegin:
                HandleCodeBlockBegin(document, token);
                break;
            case TokenType.VarKeyword:
                HandleVarKeyword(document);
                break;
            case TokenType.MethodKeyword:
                HandleMethodKeyword(document, token);
                break;
            case TokenType.ConstructorKeyword:
                HandleConstructorKeyword(document, token);
                break;
            case TokenType.NamespaceKeyword:
                HandleNamespaceKeyword(document);
                break;
            case TokenType.ClassKeyword:
                HandleClassKeyword(document);
                break;
            case TokenType.EnumKeyword:
                HandleEnumKeyword(document);
                break;
            case TokenType.Comment:
                HandleComment(document, token);
                break;
            case TokenType.SemiColon:
                HandleSemiColon(document);
                break;
            case TokenType.ClosedParathesis:
                HandleClosedParathesis(document);
                break;
            case TokenType.PublicKeyword:
            case TokenType.ProtectedKeyword:
            case TokenType.PrivateKeyword:
            case TokenType.AssemblyKeyword:
                HandleVisibilityKeywords(document, token);
                break;
            default:
                _twoTokensBack = _previousToken;
                _previousToken = token;
                break;
        }
    }

    private void HandleVariableToken(Document document, ITokenWithText tokenWithText)
    {
        if (document.CurrentContainingTypeElement == ContainingTypeElement.Class && document.CurrentElement is ClassSyntaxElement)
        {
            document.AddNewElementToCurrentAndMakeCurrent(new ClassVariableDeclarationSyntaxElement()
                .WithName(tokenWithText.TokenText)
                .WithVisibility(document.LastKnownVisibilityLevel)
                .WithStaticApplied(document.LastKnownStatic)
                .WithStartSourceCodePosition(tokenWithText.LineNumber, tokenWithText.TokenIndex));
            document.LastKnownStatic = false;
        }
        document.LastKnownVariable = tokenWithText.TokenText;
    }

    private void HandleInterfaceKeyword(Document document)
    {
        if (document.IsInTypePartOfFile)
        {
            document.AddNewElementToCurrentAndMakeCurrent(new InterfaceSyntaxElement()
                .WithInterfaceName(document.LastKnownVariable)
                .WithVisibility(document.LastKnownVisibilityLevel));
            document.LastKnownVisibilityLevel = VisibilityLevel.None;
            document.CurrentContainingTypeElement = ContainingTypeElement.Interface;
        }
    }

    private void HandleOpenBracket(Document document, IToken token)
    {
        if (document.LastKnownInCodeBlock) return;
        document.AddNewElementToCurrentAndMakeCurrent(new AttributeSyntaxElement()
            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
    }

    private void HandleClosedBracket(Document document)
    {
        if (document.LastKnownInCodeBlock) return;
        if (document.CurrentElement is AttributeSyntaxElement)
        {
            document.CurrentElement.FinishSyntaxElement(document);
            document.returnFromCurrentScope();
        }
    }

    private void HandleImplementationSection(Document document)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new ImplementationSectionSyntaxElement());
        document.IsInImplementationPartOfFile = true;
    }

    private void HandleEventKeyword(Document document, IToken token)
    {
        if (document.CurrentScope is InterfaceSyntaxElement ise)
        {
            document.CurrentContainingTypeElement = ContainingTypeElement.Interface;
            ise.ElementIsFinished = true;
        }
        document.AddNewElementToCurrentAndMakeCurrent(new EventSyntaxElement()
            .WithVisibility(document.LastKnownVisibilityLevel)
            .WithStaticApplied(document.LastKnownStatic)
            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
    }

    private void HandlePropertyKeyword(Document document, IToken token)
    {
        if (document.CurrentScope is InterfaceSyntaxElement ise)
        {
            document.CurrentContainingTypeElement = ContainingTypeElement.Interface;
            ise.ElementIsFinished = true;
        }
        if (document.CurrentContainingTypeElement == ContainingTypeElement.Class || document.CurrentContainingTypeElement == ContainingTypeElement.Record)
        {
            document.AddNewElementToCurrentAndMakeCurrent(new PropertySyntaxElement()
                .WithVisibility(document.LastKnownVisibilityLevel)
                .WithStaticApplied(document.LastKnownStatic)
                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
        }
        else if (document.CurrentContainingTypeElement == ContainingTypeElement.Interface)
        {
            document.AddNewElementToCurrentAndMakeCurrent(new PropertySyntaxInInterfaceElement()
                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
        }
        document.LastKnownStatic = false;
    }

    private void HandleConstKeyword(Document document, IToken token)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new ConstantSyntaxElement()
            .WithVisibility(document.LastKnownVisibilityLevel)
            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
    }

    private void HandleCodeBlockEnd(Document document, IToken token)
    {
        if ((document.CurrentScope is MethodElement || document.CurrentScope is ConstructorSyntaxElement) && document.CurrentContainingTypeElement == ContainingTypeElement.Class)
        {
            document.returnFromCurrentScope();
        }
        var current = document.returnFromCurrentScope();
        current.OriginalSourceCodeStopLineNumber = token.LineNumber;
        current.OriginalSourceCodeStopColumnNumber = token.TokenIndex;
        current.FinishSyntaxElement(document);
    }

    private void HandleCaseKeyword(Document document, IToken token)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new CaseSyntaxElement()
            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
    }

    private void HandleTypeDeclarationKeyword(Document document, IToken token)
    {
        document.IsInTypePartOfFile = true;
        document.AddNewElementToCurrentAndMakeCurrent(new TypeSyntaxElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
    }

    private void HandleCodeBlockBegin(Document document, IToken token)
    {
        var cb = new CodeBlockSyntaxElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
        document.AddNewElementToCurrentAndMakeCurrent(cb);
        document.LastKnownInCodeBlock = true;
    }

    private void HandleVarKeyword(Document document)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new TypeInferanceDeclarationSyntaxElement());
    }

    private void HandleMethodKeyword(Document document, IToken token)
    {
        if (document.LastKnownInCodeBlock) return;
        if (document.IsInImplementationPartOfFile)
        {
            var me = new MethodImplementationElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
            document.AddNewElementToCurrentAndMakeCurrent(me);
        }
        else
        {
            if (document.CurrentScope is InterfaceSyntaxElement ise)
            {
                document.CurrentContainingTypeElement = ContainingTypeElement.Interface;
                ise.ElementIsFinished = true;
            }
            var me = new MethodElement()
                .WithVisibility(document.LastKnownVisibilityLevel)
                .WithStaticApplied(document.LastKnownStatic)
                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
            document.AddNewElementToCurrentAndMakeCurrent(me);
            document.LastKnownStatic = false;
        }
    }

    private void HandleConstructorKeyword(Document document, IToken token)
    {
        if (document.LastKnownInCodeBlock) return;
        if (document.IsInImplementationPartOfFile)
        {
            var me = new ConstructorSyntaxImplmentationElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
            document.AddNewElementToCurrentAndMakeCurrent(me);
        }
        else
        {
            var className = document.CurrentScope is ClassSyntaxElement cse ? cse.ClassName : string.Empty;
            var me = new ConstructorSyntaxElement()
                .WithVisibility(document.LastKnownVisibilityLevel)
                .WithStaticApplied(document.LastKnownStatic)
                .WithClassName(className)
                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
            document.AddNewElementToCurrentAndMakeCurrent(me);
            document.LastKnownStatic = false;
        }
    }

    private void HandleNamespaceKeyword(Document document)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new NameSpaceElement());
    }

    private void HandleClassKeyword(Document document)
    {
        if (!(document.CurrentContainingTypeElement == ContainingTypeElement.Class || document.IsInImplementationPartOfFile))
        {
            document.AddNewElementToCurrentAndMakeCurrent(new ClassSyntaxElement()
                .WithClassName(document.LastKnownVariable)
                .WithVisibility(document.LastKnownVisibilityLevel)
                .WithStaticApplied(document.LastKnownStatic));
            document.LastKnownStatic = false;
            document.LastKnownVisibilityLevel = VisibilityLevel.None;
        }
    }

    private void HandleEnumKeyword(Document document)
    {
        document.AddNewElementToCurrentAndMakeCurrent(new EnumSyntaxElement()
            .WithEnumName(document.LastKnownVariable)
            .WithVisibility(document.LastKnownVisibilityLevel));
        document.LastKnownStatic = false;
        document.LastKnownVisibilityLevel = VisibilityLevel.None;
    }

    private void HandleComment(Document document, IToken token)
    {
        var ce = new CommentSyntaxElement
        {
            CommentType = ((CommentToken)token).CommentType
        };
        ce.CommentLines.Add(((CommentToken)token).TokenText);
        document.AddNewElementToCurrent(ce);
    }

    private void HandleSemiColon(Document document)
    {
        if (document.CurrentScope is TypeInferanceDeclarationSyntaxElement || 
            document.CurrentScope is ConstantSyntaxElement || 
            document.CurrentScope is ClassVariableDeclarationSyntaxElement)
        {
            document.returnFromCurrentScope();
        }
    }

    private void HandleClosedParathesis(Document document)
    {
        if (document.CurrentScope is ClassSyntaxElement cse)
        {
            document.CurrentContainingTypeElement = ContainingTypeElement.Class;
            cse.ElementIsFinished = true;
        }

        if (document.CurrentScope is InterfaceSyntaxElement ise)
        {
            document.CurrentContainingTypeElement = ContainingTypeElement.Interface;
            ise.ElementIsFinished = true;
        }
    }

    private void HandleVisibilityKeywords(Document document, IToken token)
    {
        switch (token.TokenType)
        {
            case TokenType.PublicKeyword:
                document.LastKnownVisibilityLevel = VisibilityLevel.Public;
                break;
            case TokenType.ProtectedKeyword:
                document.LastKnownVisibilityLevel = VisibilityLevel.Protected;
                break;
            case TokenType.PrivateKeyword:
                document.LastKnownVisibilityLevel = VisibilityLevel.Private;
                break;
            case TokenType.AssemblyKeyword:
                document.LastKnownVisibilityLevel = VisibilityLevel.Assembly;
                break;
        }

        if (document.CurrentScope is ClassSyntaxElement cse)
        {
            document.CurrentContainingTypeElement = ContainingTypeElement.Class;
            cse.ElementIsFinished = true;
        }
    }
}
