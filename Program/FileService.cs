using System;
using System.IO;
using System.Globalization;
using System.Text;

namespace Matrix;

public class FileService
{
    private readonly InputValidator validator = new();

    public void SaveReport(string filePath, Matrix original,
        Matrix? gaussInverse, TimeSpan? gaussTime,
        Matrix? blockInverse, TimeSpan? blockTime,
        Matrix? verification)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine("  ЗВІТ: Обернення матриці");
        sb.AppendLine($"  Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine("═══════════════════════════════════════════");

        sb.AppendLine();
        sb.AppendLine($"Розмірність: {original.Rows}x{original.Cols}");
        sb.AppendLine();
        sb.AppendLine("--- Вхідна матриця ---");
        AppendMatrix(sb, original);

        if (gaussInverse != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- Обернена матриця (метод Гаусса) ---");
            AppendMatrix(sb, gaussInverse);
            if (gaussTime.HasValue)
                sb.AppendLine($"Час виконання: {gaussTime.Value.TotalMilliseconds:F4} мс");
        }

        if (blockInverse != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- Обернена матриця (блочний метод) ---");
            AppendMatrix(sb, blockInverse);
            if (blockTime.HasValue)
                sb.AppendLine($"Час виконання: {blockTime.Value.TotalMilliseconds:F4} мс");
        }

        if (verification != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- Перевірка: A · A⁻¹ (має бути одинична) ---");
            AppendMatrix(sb, verification);
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    private void AppendMatrix(StringBuilder sb, Matrix matrix)
    {
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Cols; j++)
            {
                double val = matrix[i, j];
                if (Math.Abs(val) < 1e-10) val = 0.0;
                sb.Append($"{val,14:G6} ");
            }
            sb.AppendLine();
        }
    }
}
