namespace Matrix;

public class Matrix
{
    private double[,] matrix = null!;

    public Matrix(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0 || rows > 50 || cols > 50)
        {
            throw new MatrixException("Розміри матриці повинні бути від 1 до 50.");
        }

        Rows = rows;
        Cols = cols;
        InitMatrix();
    }

    public int Rows
    {
        get;
        private set;
    }

    public int Cols
    {
        get;
        private set;
    }

    private void InitMatrix()
    {
        this.matrix = new double[Rows, Cols];
    }

    public double this[int row, int col]
    {
        get
        {
            ValidateIndices(row, col);
            return matrix[row, col];
        }
        set
        {
            ValidateIndices(row, col);
            this.matrix[row, col] = value;
        }
    }

    public void IsSquare()
    {
        if (Rows != Cols)
        {
            throw new InvalidMatrixDimensionsException();
        }
    }

    public static Matrix Multiply(Matrix a, Matrix b)
    {
        if (a.Cols != b.Rows)
        {
            throw new MatrixException("Кількість стовпців першої матриці повинна дорівнювати кількості рядків другої.");
        }

        Matrix result = new Matrix(a.Rows, b.Cols);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Cols; j++)
            {
                double sum = 0;
                for (int k = 0; k < a.Cols; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    public static Matrix CreateIdentity(int n)
    {
        Matrix identity = new Matrix(n, n);
        for (int i = 0; i < n; i++)
        {
            identity[i, i] = 1.0;
        }
        return identity;
    }

    public static Matrix GenerateRandom(int n, double min, double max)
    {
        if (n <= 0)
        {
            throw new MatrixException("Розмірність повинна бути більшою за нуль.");
        }

        Random random = new Random();
        Matrix result = new Matrix(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = Math.Round(random.NextDouble() * (max - min) + min, 2);
            }
        }
        return result;
    }

    private void ValidateIndices(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols)
        {
            throw new MatrixException($"Індекс [{row}, {col}] виходить за межі матриці.");
        }
    }
}