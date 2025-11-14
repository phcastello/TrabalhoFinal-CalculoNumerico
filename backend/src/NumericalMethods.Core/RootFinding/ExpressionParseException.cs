using System;

namespace NumericalMethods.Core.RootFinding;

public sealed class ExpressionParseException : Exception
{
    public ExpressionParseException(string message) : base(message)
    {
    }
}
