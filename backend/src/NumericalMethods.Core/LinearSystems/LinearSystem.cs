namespace NumericalMethods.Core.LinearSystems;

public sealed class LinearSystem
{
    public double[,] A { get; }
    public double[] B { get; }

    public int N => B.Length;

    public LinearSystem(double[,] a, double[] b)
    {
        if (a is null)
        {
            throw new ArgumentException("Matrix A cannot be null.", nameof(a));
        }

        if (b is null)
        {
            throw new ArgumentException("Vector b cannot be null.", nameof(b));
        }

        var rows = a.GetLength(0);
        var cols = a.GetLength(1);

        if (rows != cols)
        {
            throw new ArgumentException("Matrix A must be square.", nameof(a));
        }

        if (rows != b.Length)
        {
            throw new ArgumentException("Vector b length must match the dimensions of matrix A.", nameof(b));
        }

        A = a;
        B = b;
    }
}
