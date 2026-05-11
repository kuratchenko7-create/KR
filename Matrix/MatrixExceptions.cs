namespace Matrix;
public class MatrixException : Exception
{
    public MatrixException()
        : base() { }
    public MatrixException(string message)
        : base(message) { }
    public MatrixException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class InvalidMatrixDimensionsException : MatrixException
{
    public InvalidMatrixDimensionsException() 
        : base("Матриця повинна бути квадратною для знаходження оберненої.") { }
        
    public InvalidMatrixDimensionsException(string message) 
        : base(message) { }
}

public class DegenerateMatrixException : MatrixException
{
    public DegenerateMatrixException() 
        : base("Матриця є виродженою (визначник дорівнює нулю). Обернена матриця не існує.") { }
        
    public DegenerateMatrixException(string message) 
        : base(message) { }
}