using System;

namespace Matrix;

public class InputValidator
{
    public int ValidateDimension(string input)
    {
        if (!int.TryParse(input, out int dimension))
        {
            throw new MatrixException("Некоректний формат вводу. Очікується ціле число.");
        }

        if (dimension <= 0 || dimension > 50)
        {
            throw new MatrixException("Розмірність повинна бути від 1 до 50.");
        }

        return dimension;
    }

    public double ValidateElement(string input)
    {
        if (!double.TryParse(input, out double element))
        {
            throw new MatrixException("Некоректний формат вводу. Очікується дійсне число.");
        }

        if (element != 0 && (Math.Abs(element) > 1000000 || Math.Abs(element) < 1e-5))
        {
            throw new MatrixException($"Елемент повинен бути 0 або в діапазоні [1e-5, 1000000] за модулем: {element}");
        }

        return element;
    }
}