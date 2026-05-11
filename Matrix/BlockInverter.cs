namespace Matrix;

public class BlockInverter : MatrixInverter
{
    private readonly GaussInverter gaussInverter = new();

    public Matrix Invert(Matrix matrix)
    {
        matrix.IsSquare();
        int n = matrix.Rows;

        if (n == 1)
        {
            if (Math.Abs(matrix[0, 0]) < 1e-10)
            {
                throw new DegenerateMatrixException();
            }

            Matrix result = new Matrix(1, 1);
            result[0, 0] = 1.0 / matrix[0, 0];
            return result;
        }

        int k = n / 2;
        int m = n - k;

        Matrix A = ExtractBlock(matrix, 0, 0, k, k);
        Matrix B = ExtractBlock(matrix, 0, k, k, m);
        Matrix C = ExtractBlock(matrix, k, 0, m, k);
        Matrix D = ExtractBlock(matrix, k, k, m, m);

        Matrix AInv = gaussInverter.Invert(A);

        Matrix CAInv = MultiplyBlocks(C, AInv);
        Matrix CAInvB = MultiplyBlocks(CAInv, B);
        Matrix S = SubtractBlocks(D, CAInvB);

        Matrix SInv = gaussInverter.Invert(S);

        Matrix AInvB = MultiplyBlocks(AInv, B);
        Matrix E = MultiplyBlocks(AInvB, SInv);

        Matrix topLeft = AddBlocks(AInv, MultiplyBlocks(E, CAInv));
        Matrix topRight = NegateBlock(E);
        Matrix bottomLeft = NegateBlock(MultiplyBlocks(SInv, CAInv));
        Matrix bottomRight = SInv;

        return AssembleBlocks(topLeft, topRight, bottomLeft, bottomRight, n);
    }

    private static Matrix ExtractBlock(Matrix source, int rowStart, int colStart, int rows, int cols)
    {
        Matrix block = new Matrix(rows, cols);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                block[i, j] = source[rowStart + i, colStart + j];
        return block;
    }

    private static Matrix MultiplyBlocks(Matrix a, Matrix b)
    {
        Matrix result = new Matrix(a.Rows, b.Cols);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < b.Cols; j++)
            {
                double sum = 0;
                for (int p = 0; p < a.Cols; p++)
                    sum += a[i, p] * b[p, j];
                result[i, j] = sum;
            }
        return result;
    }

    private static Matrix SubtractBlocks(Matrix a, Matrix b)
    {
        Matrix result = new Matrix(a.Rows, a.Cols);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Cols; j++)
                result[i, j] = a[i, j] - b[i, j];
        return result;
    }

    private static Matrix AddBlocks(Matrix a, Matrix b)
    {
        Matrix result = new Matrix(a.Rows, a.Cols);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Cols; j++)
                result[i, j] = a[i, j] + b[i, j];
        return result;
    }

    private static Matrix NegateBlock(Matrix a)
    {
        Matrix result = new Matrix(a.Rows, a.Cols);
        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Cols; j++)
                result[i, j] = -a[i, j];
        return result;
    }

    private static Matrix AssembleBlocks(Matrix topLeft, Matrix topRight,
        Matrix bottomLeft, Matrix bottomRight, int n)
    {
        int k = topLeft.Rows;
        int m = bottomRight.Rows;
        Matrix result = new Matrix(n, n);

        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++) result[i, j] = topLeft[i, j];
            for (int j = 0; j < m; j++) result[i, k + j] = topRight[i, j];
        }
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < k; j++) result[k + i, j] = bottomLeft[i, j];
            for (int j = 0; j < m; j++) result[k + i, k + j] = bottomRight[i, j];
        }
        return result;
    }
}
