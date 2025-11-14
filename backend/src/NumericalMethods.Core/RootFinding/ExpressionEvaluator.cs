using System;
using System.Collections.Generic;
using System.Globalization;

namespace NumericalMethods.Core.RootFinding;

public static class ExpressionEvaluator
{
    private const string VariableToken = "x";

    private enum TokenType
    {
        Number,
        Variable,
        Operator,
        LeftParenthesis,
        RightParenthesis,
        Function
    }

    private readonly struct Token
    {
        public Token(TokenType type, string text, double numberValue = 0)
        {
            Type = type;
            Text = text;
            NumberValue = numberValue;
        }

        public TokenType Type { get; }
        public string Text { get; }
        public double NumberValue { get; }
    }

    private static readonly Dictionary<string, int> OperatorPrecedence = new(StringComparer.OrdinalIgnoreCase)
    {
        {"+", 1},
        {"-", 1},
        {"*", 2},
        {"/", 2},
        {"^", 3}
    };

    private static readonly HashSet<string> RightAssociativeOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "^"
    };


    public static Func<double, double> Compile(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ExpressionParseException("Expressão vazia.");
        }

        var tokens = Tokenize(expression);
        var rpn = ConvertToReversePolish(tokens);
        var compiledTokens = rpn.ToArray();

        return x => Evaluate(compiledTokens, x);
    }

    private static IList<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var index = 0;
        var expectUnary = true;

        while (index < expression.Length)
        {
            var current = expression[index];

            if (char.IsWhiteSpace(current))
            {
                index++;
                continue;
            }

            if (char.IsDigit(current) || current == '.')
            {
                var start = index;
                index++;

                while (index < expression.Length && (char.IsDigit(expression[index]) || expression[index] == '.'))
                {
                    index++;
                }

                if (index < expression.Length && (expression[index] == 'e' || expression[index] == 'E'))
                {
                    var exponentIndex = index + 1;
                    if (exponentIndex < expression.Length && (expression[exponentIndex] == '+' || expression[exponentIndex] == '-'))
                    {
                        exponentIndex++;
                    }

                    while (exponentIndex < expression.Length && char.IsDigit(expression[exponentIndex]))
                    {
                        exponentIndex++;
                    }

                    index = exponentIndex;
                }

                var tokenText = expression[start..index];
                if (!double.TryParse(tokenText, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    throw new ExpressionParseException($"Número inválido: {tokenText}");
                }

                tokens.Add(new Token(TokenType.Number, tokenText, number));
                expectUnary = false;
                continue;
            }

            if (char.IsLetter(current))
            {
                var start = index;
                index++;
                while (index < expression.Length && char.IsLetter(expression[index]))
                {
                    index++;
                }

                var name = expression[start..index];
                if (string.Equals(name, VariableToken, StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new Token(TokenType.Variable, VariableToken));
                    expectUnary = false;
                }
                else
                {
                    var lower = name.ToLowerInvariant();
                    if (!IsSupportedFunction(lower))
                    {
                        throw new ExpressionParseException($"Função desconhecida: {name}");
                    }

                    tokens.Add(new Token(TokenType.Function, lower));
                    expectUnary = true;
                }

                continue;
            }

            if (current == '(')
            {
                tokens.Add(new Token(TokenType.LeftParenthesis, "("));
                index++;
                expectUnary = true;
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new Token(TokenType.RightParenthesis, ")"));
                index++;
                expectUnary = false;
                continue;
            }

            if (IsOperator(current))
            {
                var op = current.ToString();
                index++;

                if (expectUnary && (op == "+" || op == "-"))
                {
                    tokens.Add(new Token(TokenType.Function, op == "-" ? "neg" : "pos"));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Operator, op));
                }

                expectUnary = true;
                continue;
            }

            throw new ExpressionParseException($"Token inválido na posição {index}: '{current}'.");
        }

        return tokens;
    }

    private static bool IsOperator(char c) => c is '+' or '-' or '*' or '/' or '^';

    private static IList<Token> ConvertToReversePolish(IList<Token> tokens)
    {
        var output = new List<Token>();
        var stack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                case TokenType.Variable:
                    output.Add(token);
                    break;
                case TokenType.Function:
                    stack.Push(token);
                    break;
                case TokenType.Operator:
                    while (stack.Count > 0 &&
                           (stack.Peek().Type == TokenType.Function ||
                               (stack.Peek().Type == TokenType.Operator &&
                                (IsLeftAssociative(token.Text) && Precedence(token.Text) <= Precedence(stack.Peek().Text) ||
                                 !IsLeftAssociative(token.Text) && Precedence(token.Text) < Precedence(stack.Peek().Text)))))
                    {
                        output.Add(stack.Pop());
                    }

                    stack.Push(token);
                    break;
                case TokenType.LeftParenthesis:
                    stack.Push(token);
                    break;
                case TokenType.RightParenthesis:
                    var foundLeft = false;
                    while (stack.Count > 0)
                    {
                        var top = stack.Pop();
                        if (top.Type == TokenType.LeftParenthesis)
                        {
                            foundLeft = true;
                            break;
                        }

                        output.Add(top);
                    }

                    if (!foundLeft)
                    {
                        throw new ExpressionParseException("Parênteses desbalanceados.");
                    }

                    if (stack.Count > 0 && stack.Peek().Type == TokenType.Function)
                    {
                        output.Add(stack.Pop());
                    }

                    break;
            }
        }

        while (stack.Count > 0)
        {
            var token = stack.Pop();
            if (token.Type is TokenType.LeftParenthesis or TokenType.RightParenthesis)
            {
                throw new ExpressionParseException("Parênteses desbalanceados.");
            }

            output.Add(token);
        }

        return output;
    }

    private static int Precedence(string op)
    {
        if (!OperatorPrecedence.TryGetValue(op, out var precedence))
        {
            return 0;
        }

        return precedence;
    }

    private static bool IsLeftAssociative(string op) => !RightAssociativeOperators.Contains(op);

    private static bool IsSupportedFunction(string name)
    {
        return name is "sin" or "cos" or "tan" or "exp" or "ln" or "log10" or "sqrt";
    }

    private static double Evaluate(Token[] rpn, double x)
    {
        var stack = new Stack<double>();

        foreach (var token in rpn)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    stack.Push(token.NumberValue);
                    break;
                case TokenType.Variable:
                    stack.Push(x);
                    break;
                case TokenType.Operator:
                    if (stack.Count < 2)
                    {
                        throw new ExpressionParseException("Operadores insuficientes na expressão.");
                    }

                    var right = stack.Pop();
                    var left = stack.Pop();
                    stack.Push(ApplyOperator(token.Text, left, right));
                    break;
                case TokenType.Function:
                    if (stack.Count < 1)
                    {
                        throw new ExpressionParseException("Função com operandos insuficientes.");
                    }

                    var value = stack.Pop();
                    stack.Push(ApplyFunction(token.Text, value));
                    break;
            }
        }

        if (stack.Count != 1)
        {
            throw new ExpressionParseException("Expressão inválida para avaliação.");
        }

        return stack.Pop();
    }

    private static double ApplyOperator(string op, double left, double right) => op switch
    {
        "+" => left + right,
        "-" => left - right,
        "*" => left * right,
        "/" => right == 0 ? throw new ExpressionParseException("Divisão por zero na expressão.") : left / right,
        "^" => Math.Pow(left, right),
        _ => throw new ExpressionParseException($"Operador desconhecido: {op}")
    };

    private static double ApplyFunction(string function, double value)
    {
        return function switch
        {
            "sin" => Math.Sin(value),
            "cos" => Math.Cos(value),
            "tan" => Math.Tan(value),
            "exp" => Math.Exp(value),
            "ln" => Math.Log(value),
            "log10" => Math.Log10(value),
            "sqrt" => Math.Sqrt(value),
            "neg" => -value,
            "pos" => value,
            _ => throw new ExpressionParseException($"Função desconhecida: {function}")
        };
    }
}
