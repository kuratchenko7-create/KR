using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Matrix
{

public partial class UserInterface : Window
{
    private readonly Program program = new();
    private readonly InputValidator validator = new();

    private Matrix? currentMatrix;
    private Matrix? gaussInverse;
    private Matrix? blockInverse;
    private TimeSpan? gaussTime;
    private TimeSpan? blockTime;
    private Matrix? verificationResult;

    public UserInterface()
    {
        InitializeComponent();
    }

    private void BtnManualInput_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int n = validator.ValidateDimension(txtDimension.Text);
            currentMatrix = new Matrix(n, n);
            DisplayEditableMatrix(gridInput, currentMatrix);
            gridInput.IsReadOnly = false;
            ResetResults();
            btnInvert.IsEnabled = true;
            SetStatus($"Введіть елементи матриці {n}×{n} у таблицю.");
        }
        catch (MatrixException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BtnRandom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int n = validator.ValidateDimension(txtDimension.Text);
            currentMatrix = program.GenerateRandomMatrix(n, -100, 100);
            DisplayMatrix(gridInput, currentMatrix);
            gridInput.IsReadOnly = true;
            ResetResults();
            btnInvert.IsEnabled = true;
            SetStatus($"Згенеровано випадкову матрицю {n}×{n}.");
        }
        catch (MatrixException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BtnInvert_Click(object sender, RoutedEventArgs e)
    {
        if (currentMatrix == null)
        {
            ShowError("Спочатку введіть або завантажте матрицю.");
            return;
        }

        try
        {
            if (!gridInput.IsReadOnly)
            {
                gridInput.CommitEdit(DataGridEditingUnit.Cell, true);
                gridInput.CommitEdit(DataGridEditingUnit.Row, true);
                ReadMatrixFromGrid();
            }

            ResetResults();
            string perfText = "";

            if (rbGauss.IsChecked == true || rbBoth.IsChecked == true)
            {
                var (inv, time) = program.InvertMatrix(currentMatrix, new GaussInverter());
                gaussInverse = inv;
                gaussTime = time;
                DisplayMatrix(gridGauss, gaussInverse);
                txtGaussTime.Text = $"Час: {time.TotalMilliseconds:F4} мс";
                panelGauss.Visibility = Visibility.Visible;
                perfText += $"Гаусс: {time.TotalMilliseconds:F4} мс\n";
                perfText += $"  Складність: O(n³), n={currentMatrix.Rows}\n";
            }

            if (rbBlock.IsChecked == true || rbBoth.IsChecked == true)
            {
                var (inv, time) = program.InvertMatrix(currentMatrix, new BlockInverter());
                blockInverse = inv;
                blockTime = time;
                DisplayMatrix(gridBlock, blockInverse);
                txtBlockTime.Text = $"Час: {time.TotalMilliseconds:F4} мс";
                panelBlock.Visibility = Visibility.Visible;
                perfText += $"Блочний: {time.TotalMilliseconds:F4} мс\n";
                perfText += $"  Складність: O(n³), n={currentMatrix.Rows}\n";
            }

            txtPerformance.Text = perfText.TrimEnd();
            btnVerify.IsEnabled = true;
            btnSave.IsEnabled = true;
            SetStatus("Обернення виконано успішно.");
        }
        catch (DegenerateMatrixException)
        {
            ShowError("Матриця є виродженою (det = 0). Обернена матриця не існує.");
        }
        catch (InvalidMatrixDimensionsException)
        {
            ShowError("Матриця повинна бути квадратною.");
        }
        catch (MatrixException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError($"Непередбачена помилка: {ex.Message}");
        }
    }

    private void BtnVerify_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Matrix? inverse = gaussInverse ?? blockInverse;
            if (currentMatrix == null || inverse == null)
            {
                ShowError("Спочатку виконайте обернення матриці.");
                return;
            }

            verificationResult = program.VerifyInversion(currentMatrix, inverse);
            DisplayMatrix(gridVerify, verificationResult);
            panelVerify.Visibility = Visibility.Visible;
            SetStatus("Перевірка виконана. Результат A·A⁻¹ відображено (має бути одиничною матрицею).");
        }
        catch (Exception ex)
        {
            ShowError($"Помилка перевірки: {ex.Message}");
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (currentMatrix == null)
        {
            ShowError("Немає даних для збереження.");
            return;
        }

        try
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Зберегти результати",
                Filter = "Текстові файли (*.txt)|*.txt",
                FileName = "результат_обернення.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                program.SaveReport(dialog.FileName, currentMatrix,
                    gaussInverse, gaussTime, blockInverse, blockTime, verificationResult);
                SetStatus($"Результати збережено у файл: {dialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Помилка збереження: {ex.Message}");
        }
    }

    private void DisplayMatrix(DataGrid grid, Matrix matrix)
    {
        DataTable table = MatrixToDataTable(matrix);
        grid.ItemsSource = table.DefaultView;
        grid.IsReadOnly = true;
    }

    private void DisplayEditableMatrix(DataGrid grid, Matrix matrix)
    {
        DataTable table = MatrixToDataTable(matrix);
        grid.ItemsSource = table.DefaultView;
        grid.IsReadOnly = false;
    }

    private DataTable MatrixToDataTable(Matrix matrix)
    {
        DataTable table = new DataTable();

        for (int j = 0; j < matrix.Cols; j++)
        {
            table.Columns.Add($"C{j}", typeof(string));
        }

        for (int i = 0; i < matrix.Rows; i++)
        {
            DataRow row = table.NewRow();
            for (int j = 0; j < matrix.Cols; j++)
            {
                double val = matrix[i, j];
                if (Math.Abs(val) < 1e-9) val = 0;
                row[j] = val.ToString("G8", CultureInfo.InvariantCulture);
            }
            table.Rows.Add(row);
        }

        return table;
    }

    private void ReadMatrixFromGrid()
    {
        if (currentMatrix == null) return;

        DataView? view = gridInput.ItemsSource as DataView;
        if (view == null) return;

        DataTable table = view.Table!;
        int n = currentMatrix.Rows;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                object? val = table.Rows[i][j];
                if (val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()))
                {
                    throw new MatrixException($"Комірка [{i + 1}, {j + 1}] не заповнена.");
                }
                string text = val.ToString()!.Trim().Replace(',', '.');
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    throw new MatrixException($"Комірка [{i + 1}, {j + 1}] містить некоректне значення.");
                }
                currentMatrix[i, j] = parsed;
            }
        }
    }

    private void GridInput_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (e.EditingElement is TextBox textBox)
        {
            textBox.MaxLength = 15;
        }
    }

    private void GridInput_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            TextBox? textBox = e.EditingElement as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text.Trim().Replace(',', '.');
                textBox.Text = text;

                if (!string.IsNullOrEmpty(text) && !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                {
                    ShowError("Некоректне значення. Введіть числове значення.");
                    e.Cancel = true;
                    return;
                }

                if (!string.IsNullOrEmpty(text) && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out val) && val != 0
                    && (Math.Abs(val) > 1000000 || Math.Abs(val) < 1e-5))
                {
                    ShowError("Значення повинно бути 0 або в діапазоні [1e-5, 1000000] за модулем.");
                    e.Cancel = true;
                }
            }
        }
    }

    private void ResetResults()
    {
        gaussInverse = null;
        blockInverse = null;
        gaussTime = null;
        blockTime = null;
        verificationResult = null;

        panelGauss.Visibility = Visibility.Collapsed;
        panelBlock.Visibility = Visibility.Collapsed;
        panelVerify.Visibility = Visibility.Collapsed;
        btnVerify.IsEnabled = false;
        btnSave.IsEnabled = false;
        txtPerformance.Text = "—";
    }

    private void SetStatus(string message)
    {
        txtStatus.Text = message;
    }

    private void ShowError(string message)
    {
        MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        SetStatus($"Помилка: {message}");
    }
}
}
