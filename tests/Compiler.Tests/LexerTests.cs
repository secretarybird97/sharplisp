using SharpLisp.Common;

namespace SharpLisp.Compiler.Tests;

public class LexerTests
{
    [Fact]
    public void Static_tokenize_should_equal_next_token_sequence()
    {
        const string input = "(fn square (x) (* x x))";

        Assert.Equal(Lexer.Tokenize(input), TokenizeWithLexerInstance(new Lexer(input)));
    }

    private static IEnumerable<Token> TokenizeWithLexerInstance(Lexer lexer)
    {
        Token token;
        while ((token = lexer.NextToken()) != Token.EOF)
        {
            yield return token;
        }

        yield return token;
    }
}
