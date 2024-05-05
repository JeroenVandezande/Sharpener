using FluentAssertions;
using Sharpener;
using Sharpener.Enums;
using Sharpener.TokenTypes;
using System.Collections.Generic;
using Xunit;

namespace Sharpener_UnitTests
{
    public static class TokenTests_Extensions
    {
        internal static void AssertToken<T>(this Tokenizer tokenizer, int tokensIndex, int tokenLineIndex, TokenType tokenType)
        {
            tokenizer.Tokens[tokensIndex].Should().BeOfType<T>();
            tokenizer.Tokens[tokensIndex].TokenIndex.Should().Be(tokenLineIndex);
            tokenizer.Tokens[tokensIndex].TokenType.Should().Be(tokenType);
        }
    }

    public class TokenizerTests
    {
        

        [Fact]
        public void Tokenizer_Parse_LineComment()
        {
            // Arrange
            var lines = new List<string>() { "//Test Comment; IgnoreEverything after here. '\"" };
            var tokenizer = new Tokenizer();

            // Act
            tokenizer.Parse(lines);

            // Assert
            tokenizer.AssertToken<CommentToken>(0, 0, TokenType.Comment);
        }

        [Fact]
        public void Tokenizer_Parse_VarThenLineComment()
        {
            // Arrange
            var lines = new List<string>() { "var exampleVariable := 9 //Test Comment; IgnoreEverything after here. '\"" };
            var tokenizer = new Tokenizer();

            // Act
            tokenizer.Parse(lines);

            // Assert
            tokenizer.AssertToken<KeywordToken>(0, 0, TokenType.VarKeyword);
            tokenizer.AssertToken<IdentifierToken>(1, 1, TokenType.Variable);
            tokenizer.Tokens[1].As<IdentifierToken>().TokenText.Should().Be("exampleVariable");
            tokenizer.AssertToken<OperatorToken>(2, 2, TokenType.ClassDefinitionAndPropertyNullAccessor);
            // TODO: Make this a numerical LITERAL!
            // tokenizer.AssertToken<OperatorToken>(3, 0, TokenType.ClassDefinitionAndPropertyNullAccessor);
            tokenizer.AssertToken<CommentToken>(4, 4, TokenType.Comment);
        }
    }
}