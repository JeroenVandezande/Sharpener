using harpener.SyntaxTree.Scopes;
using Sharpener.Enums;
using Sharpener.SyntaxTree.Literals;
using Sharpener.SyntaxTree.Scopes;
using Sharpener.SyntaxTree.SyntaxElements;
using Sharpener.TokenTypes;

namespace Sharpener.SyntaxTree;

public class TokenParser
{
    private IToken _PreviousToken;
    private IToken _TwoTokensBack;
    public void ParseTokens(Tokenizer tonkenizer, ref Document document)
    {
        foreach (var token in tonkenizer.Tokens)
        {
            if (document.CurrentElement != null)
            {
                if (document.CurrentElement.WithToken(document, token))
                {
                    _TwoTokensBack = _PreviousToken;
                    _PreviousToken = token;
                    continue;
                }
            }
            
            var tokenWithText = token as ITokenWithText;
            switch (token.TokenType)
                {
                    case TokenType.Variable:
                    {
                        // If the last element was static then add it as a property
                        //if (document.LastKnownStatic)
                       // {
                        //    document.AddNewElementToCurrentAndMakeCurrent(new PropertySyntaxElement()
                        //            .WithVisibility(document.LastKnownVisibilityLevel)
                        //            .WithStaticApplied(document.LastKnownStatic));
                        //    document.LastKnownStatic = false;
                       // }

                        // If we are in a code block then add it as a variable
                       // if (document.CurrentElement is CodeBlockSyntaxElement)
                        //{
                       //     document.AddNewElementToCurrentAndMakeCurrent(new LiteralSyntaxElement());
                       // }
                       if (document.IsInClassPartOfFile)
                       {
                           if (document.CurrentElement is ClassSyntaxElement)
                           {
                               document.AddNewElementToCurrentAndMakeCurrent(new ClassVariableDeclarationSyntaxElement()
                                   .WithName(tokenWithText.TokenText)
                                   .WithVisibility(document.LastKnownVisibilityLevel)
                                   .WithStaticApplied(document.LastKnownStatic)
                                   .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                               document.LastKnownStatic = false;
                               break;
                           }
                           
                       }
                        
                        //document.CurrentElement.AddParameter(tokenWithText.TokenText, token.TokenType);
                        document.LastKnownVariable = tokenWithText.TokenText;

                        break;
                    }

                    case TokenType.InterfaceKeyword:
                    {
                        
                        break;
                    }
                    
                    case TokenType.NotifyPropertyChangedImplementation:
                    {
                        if (document.LastUsedProperty != null)
                        {
                            document.LastUsedProperty.HasNotifyPatternApplied = true;
                        }
                        break;
                    }
                    
                    
                    
                    case TokenType.OpenBracket:
                    {
                        if (document.LastKnownInCodeBlock) break;
                        document.AddNewElementToCurrentAndMakeCurrent(new AttributeSyntaxElement()
                            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                        break;
                    }
                    
                    case TokenType.ClosedBracket:
                    {
                        if (document.LastKnownInCodeBlock) break;
                        if (document.CurrentElement is AttributeSyntaxElement)
                        {
                            document.CurrentElement.FinishSyntaxElement(document);
                        }
                        break;
                    }

                    case TokenType.ImplementationSection:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new ImplementationSectionSyntaxElement());
                        document.IsInImplementationPartOfFile = true;
                        break;
                    }
                    
                    case TokenType.PropertyKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new PropertySyntaxElement()
                            .WithVisibility(document.LastKnownVisibilityLevel)
                            .WithStaticApplied(document.LastKnownStatic)
                            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                        document.LastKnownStatic = false;
                        break;
                    }

                    case TokenType.ConstKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new ConstantSyntaxElement()
                            .WithVisibility(document.LastKnownVisibilityLevel)
                            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                        break;
                    }
                    
                    case TokenType.CodeBlockEnd:
                    {
                        if ((document.CurrentScope is MethodElement) && document.IsInClassPartOfFile)
                        {
                            document.returnFromCurrentScope();
                        }
                        var current = document.returnFromCurrentScope();
                        current.OriginalSourceCodeStopLineNumber = token.LineNumber;
                        current.OriginalSourceCodeStopColumnNumber = token.TokenIndex;
                        current.FinishSyntaxElement(document);
                        break;
                    }

                    case TokenType.CaseKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new CaseSyntaxElement()
                            .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                        break;
                    }

                    case TokenType.TypeDeclarationKeyword:
                    {
                        if (document.CurrentElement is NameSpaceElement nse)
                        {
                            nse.ElementIsFinished = true;
                        }
                        document.AddNewElementToCurrentAndMakeCurrent(new TypeSyntaxElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex));
                        break;
                    }

                    case TokenType.CodeBlockBegin:
                    {
                        var cb = new CodeBlockSyntaxElement().WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
                        document.AddNewElementToCurrentAndMakeCurrent(cb);
                        document.LastKnownInCodeBlock = true;
                        break;
                    }

                    case TokenType.VarKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new TypeInferanceDeclarationSyntaxElement());
                        break;
                    }

                    case TokenType.NewKeyword:
                    {
                        //document.AddNewElementToCurrentAndMakeCurrent(new NewObjectSyntaxElement());
                        break;
                    }

                    case TokenType.MethodKeyword:
                    {
                        if (document.LastKnownInCodeBlock) break;
                        if (document.IsInImplementationPartOfFile)
                        {
                            var me = new MethodImplementationElement()
                                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
                            document.AddNewElementToCurrentAndMakeCurrent(me);
                        }
                        else
                        {
                            var me = new MethodElement()
                                .WithVisibility(document.LastKnownVisibilityLevel)
                                .WithStaticApplied(document.LastKnownStatic)
                                .WithStartSourceCodePosition(token.LineNumber, token.TokenIndex);
                            document.AddNewElementToCurrentAndMakeCurrent(me);
                            document.LastKnownStatic = false;
                        }

                        break;
                    }

                    case TokenType.ConstructorKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new ConstructorSyntaxElement()
                            .WithVisibility(document.LastKnownVisibilityLevel)
                            .WithStaticApplied(document.LastKnownStatic));
                        document.LastKnownStatic = false;
                        break;
                    }
                    
                    case TokenType.NamespaceKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new NameSpaceElement());
                        break;
                    }
                    case TokenType.ClassKeyword:
                    {
                        if (!(document.IsInClassPartOfFile || document.IsInImplementationPartOfFile))
                        {
                            document.AddNewElementToCurrentAndMakeCurrent(new ClassSyntaxElement()
                                .WithClassName(document.LastKnownVariable)
                                .WithVisibility(document.LastKnownVisibilityLevel)
                                .WithStaticApplied(document.LastKnownStatic));
                            document.LastKnownStatic = false;
                            document.LastKnownVisibilityLevel = VisibilityLevel.None;
                        }
                        else
                        {
                            document.LastKnownStatic = true;
                        }
                        break;
                    }
                    
                    case TokenType.EnumKeyword:
                    {
                        document.AddNewElementToCurrentAndMakeCurrent(new EnumSyntaxElement()
                            .WithEnumName(document.LastKnownVariable)
                            .WithVisibility(document.LastKnownVisibilityLevel));
                        document.LastKnownStatic = false;
                        document.LastKnownVisibilityLevel = VisibilityLevel.None;
                        break;
                    }

                    case TokenType.Comment:
                    {
                        var ce = new CommentSyntaxElement();
                        ce.CommentLines.Add(((CommentToken)token).TokenText);
                        ce.CommentType = ((CommentToken)token).CommentType;
                        document.AddNewElementToCurrent(ce);
                        break;
                    }

                    case TokenType.SemiColon:
                    {
                        if (document.CurrentScope is TypeInferanceDeclarationSyntaxElement)
                        {
                            document.returnFromCurrentScope();
                        }
                        
                        if (document.CurrentScope is ConstantSyntaxElement)
                        {
                            document.returnFromCurrentScope();
                        }
                        if (document.CurrentScope is ClassVariableDeclarationSyntaxElement)
                        {
                            document.returnFromCurrentScope();
                        }

                        break;
                    }

                    //case TokenType.OpenParathesis:
                    //{
                    //    // If the last token is class then we are allowing defining our inheritance
                    //    if (_PreviousToken.TokenType is TokenType.ClassKeyword)
                    //    {

                    //    }
                    //    break;
                    //}

                    case TokenType.ClosedParathesis:
                    {
                        /*if (document.CurrentScope is MethodElement)
                        {
                            document.CurrentElement.AddParameter(null, TokenType.ClosedParathesis);
                        }*/
                        
                        if (document.CurrentScope is ClassSyntaxElement cse)
                        {
                            document.IsInClassPartOfFile = true;
                            cse.ElementIsFinished = true;
                        }

                        break;
                    }

                    case TokenType.PublicKeyword:
                    {
                        document.LastKnownVisibilityLevel = VisibilityLevel.Public;
                        if (document.CurrentScope is ClassSyntaxElement cse)
                        {
                            document.IsInClassPartOfFile = true;
                            cse.ElementIsFinished = true;
                        }
                        break;
                    }
                    
                    case TokenType.ProtectedKeyword:
                    {
                        document.LastKnownVisibilityLevel = VisibilityLevel.Protected;
                        if (document.CurrentScope is ClassSyntaxElement cse)
                        {
                            document.IsInClassPartOfFile = true;
                            cse.ElementIsFinished = true;
                        }
                        break;
                    }
                    
                    case TokenType.PrivateKeyword:
                    {
                        document.LastKnownVisibilityLevel = VisibilityLevel.Private;
                        if (document.CurrentScope is ClassSyntaxElement cse)
                        {
                            document.IsInClassPartOfFile = true;
                            cse.ElementIsFinished = true;
                        }
                        break;
                    }
                    
                    case TokenType.AssemblyKeyword:
                    {
                        document.LastKnownVisibilityLevel = VisibilityLevel.Assembly;
                        if (document.CurrentScope is ClassSyntaxElement cse)
                        {
                            document.IsInClassPartOfFile = true;
                            cse.ElementIsFinished = true;
                        }
                        break;
                    }
                }

            _TwoTokensBack = _PreviousToken;
            _PreviousToken = token;
        }
    }
}