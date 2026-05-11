namespace Matrix;

public class GaussInverter : IMatrixInverter
{
    public Matrix Invert(Matrix matrix)
    {
        matrix.IsSquare();
        int n = matrix.Rows;

        double[,] augmented = CreateAugmentedMatrix(matrix, n);
        ForwardPart(augmented, n);
        BackwardPart(augmented, n);

        return ExtractInverseMatrix(augmented, n);
    }

    private double[,] CreateAugmentedMatrix(Matrix matrix, int n)
    {
        double[,] augmented = new double[n, 2 * n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = matrix[i, j];
            }
            augmented[i, n + i] = 1.0;
        }
        return augmented;
    }

    private void ForwardPart(double[,] augmented, int n)
    {
        for (int k = 0; k < n - 1; k++)
        {
            if (Math.Abs(augmented[k, k]) < 1e-10)
            {
                throw new DegenerateMatrixException();
            }

            for (int i = k + 1; i < n; i++)
            {
                double factor = augmented[i, k] / augmented[k, k];
                for (int j = k; j < 2 * n; j++)
                {
                    augmented[i, j] -= factor * augmented[k, j];
                }
            }
        }
    }

    private void BackwardPart(double[,] augmented, int n)
    {
        for (int i = 0; i < n; i++)
        {
            double diag = augmented[i, i];
            if (Math.Abs(diag) < 1e-10)
            {
                throw new DegenerateMatrixException();
            }
            for (int j = 0; j < 2 * n; j++)
            {
                augmented[i, j] /= diag;
            }
        }

        for (int k = n - 1; k >= 1; k--)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                double factor = augmented[i, k];
                for (int j = k; j < 2 * n; j++)
                {
                    augmented[i, j] -= factor * augmented[k, j];
                }
            }
        }
    }

    private Matrix ExtractInverseMatrix(double[,] augmented, int n)
    {
        Matrix inverse = new Matrix(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                inverse[i, j] = augmented[i, n + j];
            }
        }
        return inverse;
    }
}