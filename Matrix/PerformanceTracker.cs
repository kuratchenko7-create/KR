using System.Diagnostics;

namespace Matrix;

public class PerformanceTracker : IMatrixInverter
{
    private readonly IMatrixInverter target;

    public TimeSpan LastExecutionTime { get; private set; }

    public PerformanceTracker(IMatrixInverter target)
    {
        this.target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public Matrix Invert(Matrix matrix)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        Matrix result = target.Invert(matrix);
        
        stopwatch.Stop();
        LastExecutionTime = stopwatch.Elapsed;
        
        return result;
    }
}