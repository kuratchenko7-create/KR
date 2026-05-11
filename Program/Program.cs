using System;
using System.Windows;

namespace Matrix;

public class Program
{
    private readonly FileService fileService = new();

    [STAThread]
    public static void Main()
    {
        Application app = new Application();
        UserInterface mainWindow = new UserInterface();
        app.Run(mainWindow);
    }

    public (Matrix inverse, TimeSpan elapsed) InvertMatrix(Matrix matrix, IMatrixInverter inverter)
    {
        PerformanceTracker tracker = new PerformanceTracker(inverter);
        Matrix inverse = tracker.Invert(matrix);
        return (inverse, tracker.LastExecutionTime);
    }

    public Matrix VerifyInversion(Matrix original, Matrix inverse)
    {
        return Matrix.Multiply(original, inverse);
    }

    public Matrix GenerateRandomMatrix(int size, double min, double max)
    {
        return Matrix.GenerateRandom(size, min, max);
    }

    public void SaveReport(string filePath, Matrix original,
        Matrix? gaussInverse, TimeSpan? gaussTime,
        Matrix? blockInverse, TimeSpan? blockTime,
        Matrix? verification)
    {
        fileService.SaveReport(filePath, original, gaussInverse, gaussTime,
            blockInverse, blockTime, verification);
    }
}
