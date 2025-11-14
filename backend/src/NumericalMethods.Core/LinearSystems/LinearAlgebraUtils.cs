using System;

namespace NumericalMethods.Core.LinearSystems;

public static class LinearAlgebraUtils
{
    public static double[,] CloneMatrix(double[,] source)
    {
        var rows = source.GetLength(0);
        var cols = source.GetLength(1);
        var clone = new double[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                clone[i, j] = source[i, j];
            }
        }

        return clone;
    }

    public static double[] CloneVector(double[] source)
    {
        var clone = new double[source.Length];
        Array.Copy(source, clone, source.Length);
        return clone;
    }

    public static double[] ForwardSubstitution(double[,] L, double[] b)
    {
        var n = L.GetLength(0);
        var result = new double[n];

        for (var i = 0; i < n; i++)
        {
            double sum = 0;
            for (var j = 0; j < i; j++)
            {
                sum += L[i, j] * result[j];
            }

            var pivot = L[i, i];
            if (IsApproximatelyZero(pivot))
            {
                throw new InvalidOperationException("Pivot is zero during forward substitution.");
            }

            result[i] = (b[i] - sum) / pivot;
        }

        return result;
    }

    public static double[] BackwardSubstitution(double[,] U, double[] y)
    {
        var n = U.GetLength(0);
        var result = new double[n];

        for (var i = n - 1; i >= 0; i--)
        {
            double sum = 0;
            for (var j = i + 1; j < n; j++)
            {
                sum += U[i, j] * result[j];
            }

            var pivot = U[i, i];
            if (IsApproximatelyZero(pivot))
            {
                throw new InvalidOperationException("Pivot is zero during backward substitution.");
            }

            result[i] = (y[i] - sum) / pivot;
        }

        return result;
    }

    public static double[] Multiply(double[,] A, double[] x)
    {
        var rows = A.GetLength(0);
        var cols = A.GetLength(1);
        var result = new double[rows];

        for (var i = 0; i < rows; i++)
        {
            double sum = 0;
            for (var j = 0; j < cols; j++)
            {
                sum += A[i, j] * x[j];
            }

            result[i] = sum;
        }

        return result;
    }

    public static double Norm2(double[] v)
    {
        double sum = 0;
        foreach (var value in v)
        {
            sum += value * value;
        }

        return Math.Sqrt(sum);
    }

    public static bool IsApproximatelyZero(double value, double eps = 1e-12)
    {
        return Math.Abs(value) < eps;
    }
}
