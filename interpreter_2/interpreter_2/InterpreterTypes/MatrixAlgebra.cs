using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace interpreter_2.InterpreterTypes
{
    public class Matrix
    {
        public double[,] Data { get; set; }
        public int Rows { get; }
        public int Columns { get; }

        public Matrix(double[,] data)
        {
            Data = data;
            Rows = data.GetLength(0);
            Columns = data.GetLength(1);
        }

        public double this[int row, int col]
        {
            get => Data[row, col];
            set => Data[row, col] = value;
        }

        public Matrix Clone()
        {
            return new Matrix((double[,])Data.Clone());
        }

        public Matrix Multiply(double scalar)
        {
            double[,] result = new double[Rows, Columns];
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[i, j] = Data[i, j] * scalar;
            return new Matrix(result);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
                throw new ArgumentException("Несовместимые размеры матриц");

            double[,] result = new double[a.Rows, b.Columns];
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    for (int k = 0; k < a.Columns; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return new Matrix(result);
        }

        public Matrix Submatrix(int startRow, int startCol, int rowCount, int colCount)
        {
            double[,] sub = new double[rowCount, colCount];
            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < colCount; j++)
                    sub[i, j] = Data[startRow + i, startCol + j];
            return new Matrix(sub);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                sb.Append("(");
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(Data[i, j].ToString("0.###"));
                    if (j < Columns - 1) sb.Append(", ");
                }
                sb.Append(")");
                if (i < Rows - 1) sb.AppendLine();
            }
            return sb.ToString();
        }
    }
    public sealed class MatrixAlgebra
    {
        public MatrixAlgebra() { }
        #region notstrings

        public Matrix MatrixArithmetic(string operation, Matrix a, Matrix b = null, double scalar = 0)
        {
            Matrix result;
            switch (operation)
            {
                case "+":
                    if (a.Rows != b.Rows || a.Columns != b.Columns)
                        throw new ArgumentException("Матрицы должны быть одного размера");

                    var sum = new double[a.Rows, a.Columns];
                    for (int i = 0; i < a.Rows; i++)
                    {
                        for (int j = 0; j < a.Columns; j++)
                        {
                            sum[i, j] = a.Data[i, j] + b.Data[i, j];
                        }
                    }
                    result = new Matrix(sum);
                    break;

                case "-":
                    if (a.Rows != b.Rows || a.Columns != b.Columns)
                        throw new ArgumentException("Матрицы должны быть одного размера");

                    var dif = new double[a.Rows, a.Columns];
                    for (int i = 0; i < a.Rows; i++)
                    {
                        for (int j = 0; j < a.Columns; j++)
                        {
                            dif[i, j] = a.Data[i, j] - b.Data[i, j];
                        }
                    }
                    result = new Matrix(dif);
                    break;

                case "*":
                    if (b != null)
                    {
                        if (a.Columns != b.Rows)
                            throw new ArgumentException("Количество столбцов первой матрицы должно совпадать с количеством строк второй");

                        var product = new double[a.Rows, b.Columns];
                        for (int i = 0; i < a.Rows; i++)
                        {
                            for (int j = 0; j < b.Columns; j++)
                            {
                                product[i, j] = 0;
                                for (int k = 0; k < a.Columns; k++)
                                {
                                    product[i, j] += a.Data[i, k] * b.Data[k, j];
                                }
                            }
                        }
                        result = new Matrix(product);
                    }
                    else
                    {
                        var scaled = new double[a.Rows, a.Columns];
                        for (int i = 0; i < a.Rows; i++)
                        {
                            for (int j = 0; j < a.Columns; j++)
                            {
                                scaled[i, j] = a.Data[i, j] * scalar;
                            }
                        }
                        result = new Matrix(scaled);
                    }
                    break;

                default:
                    throw new ArgumentException("Неподдерживаемая операция");
            }

            return result;
        }

        private double CalculateDeterminant(Matrix matrix)
        {
            if (matrix.Rows == 1) return matrix.Data[0, 0];

            double det = 0;
            for (int j = 0; j < matrix.Columns; j++)
            {
                var minor = GetMinor(matrix, 0, j);
                double minorDet = CalculateDeterminant(minor);
                det += (j % 2 == 0 ? 1 : -1) * matrix.Data[0, j] * minorDet;
            }
            return det;
        }

        public double MatrixDeterminant(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            return CalculateDeterminant(matrix);
        }

        public Matrix InverseMatrix(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");
            Matrix identity;

            double det = CalculateDeterminant(matrix);

            if (Math.Abs(det) < 1e-10)
            {
                throw new ArgumentException("матрицы не существует");
            }
            else
            {
                Matrix adjugate = CreateAdjugateMatrix(matrix);
                Matrix inverse = adjugate.Multiply(1.0 / det);
                identity = matrix * inverse;
            }
            return identity;
        }

        public int MatrixRank(Matrix matrix)
        {
            Matrix reduced = matrix.Clone();
            int rank = 0;

            for (int col = 0; col < reduced.Columns && rank < reduced.Rows; col++)
            {
                int pivotRow = rank;
                for (int row = rank; row < reduced.Rows; row++)
                {
                    if (Math.Abs(reduced[row, col]) > Math.Abs(reduced[pivotRow, col]))
                        pivotRow = row;
                }

                if (Math.Abs(reduced[pivotRow, col]) < 1e-10) continue;

                if (pivotRow != rank)
                {
                    SwapRows(reduced, rank, pivotRow);
                }

                for (int row = rank + 1; row < reduced.Rows; row++)
                {
                    double factor = reduced[row, col] / reduced[rank, col];
                    for (int c = col; c < reduced.Columns; c++)
                    {
                        reduced[row, c] -= factor * reduced[rank, c];
                    }
                }
                rank++;
            }
            return rank;
        }

        public int LinearSpanDimension(Vector[] vectors)
        {
            if (vectors.Length == 0)
                throw new ArgumentException("Массив векторов не может быть пустым");

            int dimension = vectors[0].Dimension;

            Matrix matrix = new Matrix(new double[dimension, vectors.Length]);
            for (int i = 0; i < vectors.Length; i++)
            {
                if (vectors[i].Dimension != dimension)
                    throw new ArgumentException("Все векторы должны иметь одинаковую размерность");

                for (int j = 0; j < dimension; j++)
                {
                    matrix[j, i] = vectors[i].Components[j];
                }
            }
            int rank = MatrixRank(matrix);
            return rank;
        }

        public bool CheckLinearSpanMembership(Vector vector, Vector[] span)
        {
            bool belongs = CheckSpanMembership(vector, span);

            return belongs;
        }

        public Matrix SolveLinearSystem(Matrix coefficients, Matrix constants, string method)
        {
            if (coefficients.Rows != coefficients.Columns)
                throw new ArgumentException("Матрица коэффициентов должна быть квадратной");
            if (coefficients.Rows != constants.Rows)
                throw new ArgumentException("Размерности матриц не согласованы");

            Matrix augmented = CreateAugmentedMatrix(coefficients, constants);
            if (method == "Гаусс")
            {
                for (int col = 0; col < augmented.Columns - 1; col++)
                {
                    int maxRow = FindPivotRow(augmented, col);
                    if (maxRow != col)
                    {
                        SwapRows(augmented, col, maxRow);
                    }

                    for (int row = col + 1; row < augmented.Rows; row++)
                    {
                        double factor = augmented[row, col] / augmented[col, col];
                        for (int k = col; k < augmented.Columns; k++)
                        {
                            augmented[row, k] -= factor * augmented[col, k];
                        }
                    }
                }
                double[] solution = new double[augmented.Rows];

                for (int row = augmented.Rows - 1; row >= 0; row--)
                {
                    solution[row] = augmented[row, augmented.Columns - 1];
                    for (int col = row + 1; col < augmented.Columns - 1; col++)
                    {
                        solution[row] -= augmented[row, col] * solution[col];
                    }
                    solution[row] /= augmented[row, row];
                }
            }
            else if (method == "Жордан")
            {
                for (int col = 0; col < augmented.Columns - 1; col++)
                {
                    int maxRow = FindPivotRow(augmented, col);
                    if (maxRow != col)
                    {
                        SwapRows(augmented, col, maxRow);
                    }

                    double pivot = augmented[col, col];
                    for (int k = col; k < augmented.Columns; k++)
                    {
                        augmented[col, k] /= pivot;
                    }

                    for (int row = 0; row < augmented.Rows; row++)
                    {
                        if (row != col && Math.Abs(augmented[row, col]) > 1e-10)
                        {
                            double factor = augmented[row, col];
                            for (int k = col; k < augmented.Columns; k++)
                            {
                                augmented[row, k] -= factor * augmented[col, k];
                            }
                        }
                    }
                }
            }

            return augmented;
        }

        public double[] Eigenvalues(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            double[] eigenvalues = QRAlgorithm(matrix, 100, 1e-10);
            return eigenvalues;
        }

        public Vector[] Eigenvectors(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            double[] eigenvalues = QRAlgorithm(matrix, 100, 1e-6);

            Vector[] eigenvectors = new Vector[eigenvalues.Length];

            for (int i = 0; i < eigenvalues.Length; i++)
            {
                Matrix A_minus_lambdaI = MatrixSubtract(matrix, MatrixIdentity(matrix.Rows).Multiply(eigenvalues[i]));
                double[] eigenvectorComponents = FindEigenvector(A_minus_lambdaI);
                eigenvectors[i] = new Vector(eigenvectorComponents);
            }

            return eigenvectors;
        }
        #endregion






        #region strings
        public string MatrixArithmeticToString(string operation, Matrix a, Matrix b = null, double scalar = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            switch (operation)
            {
                case "+":
                    if (a.Rows != b.Rows || a.Columns != b.Columns)
                        throw new ArgumentException("Матрицы должны быть одного размера");

                    var sum = new double[a.Rows, a.Columns];
                    sb.AppendLine(@"\mathbf{A} + \mathbf{B} = ");
                    for (int i = 0; i < a.Rows; i++)
                    {
                        for (int j = 0; j < a.Columns; j++)
                        {
                            sum[i, j] = a.Data[i, j] + b.Data[i, j];
                        }
                    }
                    sb.AppendLine(MatrixToLatex(new Matrix(sum)));
                    break;

                case "*":
                    if (b != null)
                    {
                        if (a.Columns != b.Rows)
                            throw new ArgumentException("Количество столбцов первой матрицы должно совпадать с количеством строк второй");

                        var product = new double[a.Rows, b.Columns];
                        sb.AppendLine(@"\mathbf{A} \times \mathbf{B} = ");
                        for (int i = 0; i < a.Rows; i++)
                        {
                            for (int j = 0; j < b.Columns; j++)
                            {
                                product[i, j] = 0;
                                for (int k = 0; k < a.Columns; k++)
                                {
                                    product[i, j] += a.Data[i, k] * b.Data[k, j];
                                    sb.Append($@"{a.Data[i, k]} \times {b.Data[k, j]}");
                                    if (k < a.Columns - 1) sb.Append(@" + ");
                                }
                                sb.AppendLine(@" \\");
                            }
                        }
                        sb.AppendLine(MatrixToLatex(new Matrix(product)));
                    }
                    else
                    {
                        var scaled = new double[a.Rows, a.Columns];
                        
                        sb.AppendLine($@"{scalar} \times \mathbf{scaled} = ");
                        for (int i = 0; i < a.Rows; i++)
                        {
                            for (int j = 0; j < a.Columns; j++)
                            {
                                scaled[i, j] = a.Data[i, j] * scalar;
                            }
                        }
                        sb.AppendLine(MatrixToLatex(new Matrix(scaled)));
                    }
                    break;

                default:
                    throw new ArgumentException("Неподдерживаемая операция");
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string MatrixDeterminantToString(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\det(\mathbf{A}) = " + CalculateDeterminantToString(matrix, sb));
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        private double CalculateDeterminantToString(Matrix matrix, StringBuilder sb)
        {
            if (matrix.Rows == 1) return matrix.Data[0, 0];

            double det = 0;
            for (int j = 0; j < matrix.Columns; j++)
            {
                var minor = GetMinor(matrix, 0, j);
                double minorDet = CalculateDeterminantToString(minor, sb);
                det += (j % 2 == 0 ? 1 : -1) * matrix.Data[0, j] * minorDet;

                sb.AppendLine($@"{matrix.Data[0, j]} \times {minorDet} {(j % 2 == 0 ? "+" : "-")}");
            }
            return det;
        }

        public string InverseMatrixToString(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            double det = CalculateDeterminant(matrix);
            sb.AppendLine($@"\det(\mathbf{{A}}) = {det:0.###}\\");

            if (Math.Abs(det) < 1e-10)
            {
                sb.AppendLine(@"\text{Матрица вырожденная, обратной не существует}");
            }
            else
            {
                sb.AppendLine(@"\text{Присоединённая матрица:}\\");
                Matrix adjugate = CreateAdjugateMatrix(matrix);
                sb.AppendLine(MatrixToLatex(adjugate) + @"\\");

                sb.AppendLine(@"\text{Обратная матрица:}\\");
                Matrix inverse = adjugate.Multiply(1.0 / det);
                sb.AppendLine(MatrixToLatex(inverse));

                sb.AppendLine(@"\text{Проверка } \mathbf{A} \cdot \mathbf{A}^{-1} = \mathbf{I}:\\");
                Matrix identity = matrix * inverse;
                sb.AppendLine(MatrixToLatex(identity));
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string SolveLinearSystemToString(Matrix coefficients, Matrix constants, string method)
        {
            if (coefficients.Rows != coefficients.Columns)
                throw new ArgumentException("Матрица коэффициентов должна быть квадратной");
            if (coefficients.Rows != constants.Rows)
                throw new ArgumentException("Размерности матриц не согласованы");

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            // расширенная матрица
            Matrix augmented = CreateAugmentedMatrix(coefficients, constants);
            sb.AppendLine(@"\text{Расширенная матрица системы:}\\");
            sb.AppendLine(MatrixToLatex(augmented) + @"\\");

            if (method == "Gauss")
            {
                sb.AppendLine(@"\text{Метод Гаусса:}\\");
                sb.AppendLine(@"\text{Прямой ход:}\\");

                // Прямой ход метода Гаусса
                for (int col = 0; col < augmented.Columns - 1; col++)
                {
                    // Частичный выбор ведущего элемента
                    int maxRow = FindPivotRow(augmented, col);
                    if (maxRow != col)
                    {
                        SwapRows(augmented, col, maxRow);
                        sb.AppendLine($@"\text{{Меняем строки {col + 1} и {maxRow + 1}:}}\\");
                        sb.AppendLine(MatrixToLatex(augmented) + @"\\");
                    }

                    // Обнуление элементов ниже ведущего
                    for (int row = col + 1; row < augmented.Rows; row++)
                    {
                        double factor = augmented[row, col] / augmented[col, col];
                        for (int k = col; k < augmented.Columns; k++)
                        {
                            augmented[row, k] -= factor * augmented[col, k];
                        }
                        sb.AppendLine($@"\text{{Вычитаем строку {col + 1} умноженную на {factor:0.###} из строки {row + 1}:}}\\");
                        sb.AppendLine(MatrixToLatex(augmented) + @"\\");
                    }
                }

                sb.AppendLine(@"\text{Обратный ход:}\\");
                double[] solution = new double[augmented.Rows];

                // Обратный ход
                for (int row = augmented.Rows - 1; row >= 0; row--)
                {
                    solution[row] = augmented[row, augmented.Columns - 1];
                    for (int col = row + 1; col < augmented.Columns - 1; col++)
                    {
                        solution[row] -= augmented[row, col] * solution[col];
                    }
                    solution[row] /= augmented[row, row];
                    sb.AppendLine($@"x_{row + 1} &= \frac{{{augmented[row, augmented.Columns - 1]:0.###} - \sum_{{k={row + 2}}}^{{n}}({augmented[row, row]:0.###} \cdot x_k)}}{{{augmented[row, row]:0.###}}}\\");
                    sb.AppendLine($@"x_{row + 1} &= {solution[row]:0.###}\\");
                }
            }
            else if (method == "Jordan")
            {
                sb.AppendLine(@"\text{Метод Жордана-Гаусса:}\\");

                for (int col = 0; col < augmented.Columns - 1; col++)
                {
                    // Выбор ведущего элемента
                    int maxRow = FindPivotRow(augmented, col);
                    if (maxRow != col)
                    {
                        SwapRows(augmented, col, maxRow);
                        sb.AppendLine($@"\text{{Меняем строки {col + 1} и {maxRow + 1}:}}\\");
                        sb.AppendLine(MatrixToLatex(augmented) + @"\\");
                    }

                    // Нормировка ведущей строки
                    double pivot = augmented[col, col];
                    for (int k = col; k < augmented.Columns; k++)
                    {
                        augmented[col, k] /= pivot;
                    }
                    sb.AppendLine($@"\text{{Нормируем строку {col + 1} (делим на {pivot:0.###}):}}\\");
                    sb.AppendLine(MatrixToLatex(augmented) + @"\\");

                    // Обнуление элементов в столбце
                    for (int row = 0; row < augmented.Rows; row++)
                    {
                        if (row != col && Math.Abs(augmented[row, col]) > 1e-10)
                        {
                            double factor = augmented[row, col];
                            for (int k = col; k < augmented.Columns; k++)
                            {
                                augmented[row, k] -= factor * augmented[col, k];
                            }
                            sb.AppendLine($@"\text{{Вычитаем строку {col + 1} умноженную на {factor:0.###} из строки {row + 1}:}}\\");
                            sb.AppendLine(MatrixToLatex(augmented) + @"\\");
                        }
                    }
                }

                sb.AppendLine(@"\text{Решение системы:}\\");
                for (int i = 0; i < augmented.Rows; i++)
                {
                    sb.AppendLine($@"x_{i + 1} &= {augmented[i, augmented.Columns - 1]:0.###}\\");
                }
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }


        public string EigenvaluesToString(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Характеристическое уравнение: } \det(\mathbf{A} - \lambda\mathbf{I}) = 0\\");

            sb.AppendLine(@"\text{Используем QR-алгоритм для нахождения собственных чисел}\\");
            var eigenvalues = QRAlgorithm(matrix, 100, 1e-10);

            sb.AppendLine(@"\text{Собственные числа:}\\");
            for (int i = 0; i < eigenvalues.Length; i++)
            {
                sb.AppendLine($@"\lambda_{i + 1} &\approx {eigenvalues[i]:0.###}\\");
            }
            
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string EigenvectorsToString(Matrix matrix)
        {
            if (matrix.Rows != matrix.Columns)
                throw new ArgumentException("Матрица должна быть квадратной");

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            var eigenvalues = QRAlgorithm(matrix, 100, 1e-6);
            sb.AppendLine(@"\text{Найденные собственные числа:}\\");
            for (int i = 0; i < eigenvalues.Length; i++)
            {
                sb.AppendLine($@"\lambda_{i + 1} &= {eigenvalues[i]:0.###}\\");
            }

            sb.AppendLine(@"\text{Соответствующие собственные векторы:}\\");

            for (int i = 0; i < eigenvalues.Length; i++)
            {
                Matrix A_minus_lambdaI = MatrixSubtract(matrix, MatrixIdentity(matrix.Rows).Multiply(eigenvalues[i]));
                var eigenvector = FindEigenvector(A_minus_lambdaI);

                sb.AppendLine($@"\text{{Для }}\lambda_{i + 1} = {eigenvalues[i]:0.###}:\\");
                sb.AppendLine(@"\begin{pmatrix}");
                for (int j = 0; j < eigenvector.Length; j++)
                {
                    sb.Append(eigenvector[j].ToString("0.###"));
                    if (j < eigenvector.Length - 1) sb.Append(@" \\ ");
                }
                sb.AppendLine(@"\end{pmatrix}\\");
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string MatrixRankToString(Matrix matrix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            Matrix reduced = matrix.Clone();
            int rank = 0;

            sb.AppendLine(@"\text{Исходная матрица:}\\");
            sb.AppendLine(MatrixToLatex(reduced) + @"\\");
            sb.AppendLine(@"\text{Приводим к ступенчатому виду:}\\");

            for (int col = 0; col < reduced.Columns && rank < reduced.Rows; col++)
            {
                int pivotRow = rank;
                for (int row = rank; row < reduced.Rows; row++)
                {
                    if (Math.Abs(reduced[row, col]) > Math.Abs(reduced[pivotRow, col]))
                        pivotRow = row;
                }

                if (Math.Abs(reduced[pivotRow, col]) < 1e-10) continue;

                if (pivotRow != rank)
                {
                    SwapRows(reduced, rank, pivotRow);
                    sb.AppendLine($@"\text{{Меняем строки {rank + 1} и {pivotRow + 1}:}}\\");
                    sb.AppendLine(MatrixToLatex(reduced) + @"\\");
                }

                for (int row = rank + 1; row < reduced.Rows; row++)
                {
                    double factor = reduced[row, col] / reduced[rank, col];
                    for (int c = col; c < reduced.Columns; c++)
                    {
                        reduced[row, c] -= factor * reduced[rank, c];
                    }
                    sb.AppendLine($@"\text{{Обнуляем строку {row + 1} с помощью строки {rank + 1} (множитель {factor:0.###}):}}\\");
                    sb.AppendLine(MatrixToLatex(reduced) + @"\\");
                }
                rank++;
            }
            sb.AppendLine($@"\text{{Ранг матрицы}} = {rank}");
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string LinearSpanDimensionToString(Vector[] vectors)
        {
            if (vectors.Length == 0)
                throw new ArgumentException("Массив векторов не может быть пустым");

            int dimension = vectors[0].Dimension;
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            Matrix matrix = new Matrix(new double[dimension, vectors.Length]);
            for (int i = 0; i < vectors.Length; i++)
            {
                if (vectors[i].Dimension != dimension)
                    throw new ArgumentException("Все векторы должны иметь одинаковую размерность");

                for (int j = 0; j < dimension; j++)
                {
                    matrix[j, i] = vectors[i].Components[j];
                }
            }

            sb.AppendLine(@"\text{Матрица из векторов (по столбцам):}\\");
            sb.AppendLine(MatrixToLatex(matrix) + @"\\");

            int rank = MatrixRank(matrix);
            sb.AppendLine($@"\text{{Размер линейной оболочки}} = {rank}");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string CheckLinearSpanMembershipToString(Vector vector, Vector[] span)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            bool belongs = CheckSpanMembership(vector, span);

            sb.AppendLine(@"\text{Проверяем принадлежность вектора } \mathbf{v} = " +
                         VectorToLatex(vector) + @" \text{ линейной оболочке}\\");

            sb.AppendLine(@"\text{Векторы оболочки:}\\");
            foreach (var v in span)
            {
                sb.AppendLine(VectorToLatex(v) + @"\\");
            }

            sb.AppendLine(belongs
                ? @"\text{Вектор принадлежит линейной оболочке}"
                : @"\text{Вектор не принадлежит линейной оболочке}");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        #endregion


        // Вспомогательные методы
        private string MatrixToLatex(Matrix matrix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{pmatrix}");
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    sb.Append(matrix.Data[i, j]);
                    if (j < matrix.Columns - 1) sb.Append(@" & ");
                }
                if (i < matrix.Rows - 1) sb.AppendLine(@" \\");
            }
            sb.AppendLine(@"\end{pmatrix}");
            return sb.ToString();
        }

        private Matrix GetMinor(Matrix matrix, int row, int col)
        {
            var minor = new double[matrix.Rows - 1, matrix.Columns - 1];
            for (int i = 0, k = 0; i < matrix.Rows; i++)
            {
                if (i == row) continue;
                for (int j = 0, l = 0; j < matrix.Columns; j++)
                {
                    if (j == col) continue;
                    minor[k, l] = matrix.Data[i, j];
                    l++;
                }
                k++;
            }
            return new Matrix(minor);
        }

        private Matrix CreateAdjugateMatrix(Matrix matrix)
        {
            int n = matrix.Rows;
            double[,] adjugate = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double[,] minor = new double[n - 1, n - 1];
                    for (int k = 0, ki = 0; k < n; k++)
                    {
                        if (k == i) continue;
                        for (int l = 0, lj = 0; l < n; l++)
                        {
                            if (l == j) continue;
                            minor[ki, lj] = matrix[k, l];
                            lj++;
                        }
                        ki++;
                    }
                    adjugate[j, i] = Math.Pow(-1, i + j) * CalculateDeterminant(new Matrix(minor));
                }
            }

            return new Matrix(adjugate);
        }

        private void SwapRows(Matrix matrix, int row1, int row2)
        {
            for (int col = 0; col < matrix.Columns; col++)
            {
                double temp = matrix[row1, col];
                matrix[row1, col] = matrix[row2, col];
                matrix[row2, col] = temp;
            }
        }


        private bool CheckSpanMembership(Vector vector, Vector[] span)
        {
            Matrix m = new Matrix(new double[vector.Dimension, span.Length + 1]);
            for (int i = 0; i < span.Length; i++)
                for (int j = 0; j < vector.Dimension; j++)
                    m[j, i] = span[i].Components[j];

            for (int j = 0; j < vector.Dimension; j++)
                m[j, span.Length] = vector.Components[j];

            int rankBefore = MatrixRank(m.Submatrix(0, 0, m.Rows, span.Length));
            int rankAfter = MatrixRank(m);

            return rankBefore == rankAfter;
        }
        private string VectorToLatex(Vector vector)
        {
            return @"\begin{pmatrix}" + string.Join(@" \\ ", vector.Components.Select(x => x.ToString("0.###"))) + @"\end{pmatrix}";
        }

        private int FindPivotRow(Matrix matrix, int col)
        {
            int maxRow = col;
            for (int row = col + 1; row < matrix.Rows; row++)
            {
                if (Math.Abs(matrix[row, col]) > Math.Abs(matrix[maxRow, col]))
                    maxRow = row;
            }
            return maxRow;
        }

        private Matrix CreateAugmentedMatrix(Matrix coefficients, Matrix constants)
        {
            double[,] augmented = new double[coefficients.Rows, coefficients.Columns + 1];
            for (int i = 0; i < coefficients.Rows; i++)
            {
                for (int j = 0; j < coefficients.Columns; j++)
                {
                    augmented[i, j] = coefficients[i, j];
                }
                augmented[i, coefficients.Columns] = constants[i, 0];
            }
            return new Matrix(augmented);
        }

        private void QRDecomposition(Matrix matrix, out Matrix Q, out Matrix R)
        {
            int n = matrix.Rows;
            double[,] q = new double[n, n];
            double[,] r = new double[n, n];

            for (int i = 0; i < n; i++)
                q[i, i] = 1;

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    r[i, j] = matrix[i, j];

            for (int k = 0; k < n - 1; k++)
            {
                double[] x = new double[n - k];
                for (int i = k; i < n; i++)
                    x[i - k] = r[i, k];

                double normX = Math.Sqrt(x.Sum(val => val * val));
                double[] v = new double[x.Length];
                Array.Copy(x, v, x.Length);
                v[0] += Math.Sign(x[0]) * normX;

                double normV = Math.Sqrt(v.Sum(val => val * val));
                if (normV < 1e-10) continue;

                for (int i = 0; i < v.Length; i++)
                    v[i] /= normV;

                for (int j = k; j < n; j++)
                {
                    double[] column = new double[n - k];
                    for (int i = k; i < n; i++)
                        column[i - k] = r[i, j];

                    double dot = v.Zip(column, (a, b) => a * b).Sum();

                    for (int i = k; i < n; i++)
                        r[i, j] -= 2 * v[i - k] * dot;
                }

                for (int j = 0; j < n; j++)
                {
                    double[] column = new double[n - k];
                    for (int i = k; i < n; i++)
                        column[i - k] = q[i, j];

                    double dot = v.Zip(column, (a, b) => a * b).Sum();

                    for (int i = k; i < n; i++)
                        q[i, j] -= 2 * v[i - k] * dot;
                }
            }

            double[,] qT = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    qT[i, j] = q[j, i];

            Q = new Matrix(qT);
            R = new Matrix(r);
        }

        private double[] QRAlgorithm(Matrix matrix, int maxIterations, double tolerance)
        {
            Matrix A = matrix.Clone();
            for (int k = 0; k < maxIterations; k++)
            {
                QRDecomposition(A, out Matrix Q, out Matrix R);
                A = R * Q;

                bool converged = true;
                for (int i = 1; i < A.Rows; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Math.Abs(A[i, j]) > tolerance)
                        {
                            converged = false;
                            break;
                        }
                    }
                    if (!converged) break;
                }
                if (converged) break;
            }

            double[] eigenvalues = new double[A.Rows];
            for (int i = 0; i < A.Rows; i++)
                eigenvalues[i] = A[i, i];

            return eigenvalues;
        }

        private double[] FindEigenvector(Matrix A_minus_lambdaI)
        {
            Matrix augmented = new Matrix(new double[A_minus_lambdaI.Rows, A_minus_lambdaI.Columns + 1]);
            for (int i = 0; i < A_minus_lambdaI.Rows; i++)
            {
                for (int j = 0; j < A_minus_lambdaI.Columns; j++)
                    augmented[i, j] = A_minus_lambdaI[i, j];
                augmented[i, A_minus_lambdaI.Columns] = 0;
            }

            for (int col = 0; col < augmented.Columns - 1; col++)
            {
                int pivotRow = FindPivotRow(augmented, col);
                if (pivotRow != col)
                    SwapRows(augmented, col, pivotRow);

                for (int row = col + 1; row < augmented.Rows; row++)
                {
                    double factor = augmented[row, col] / augmented[col, col];
                    for (int k = col; k < augmented.Columns; k++)
                        augmented[row, k] -= factor * augmented[col, k];
                }
            }

            double[] eigenvector = new double[augmented.Rows];
            eigenvector[augmented.Rows - 1] = 1; 

            for (int row = augmented.Rows - 2; row >= 0; row--)
            {
                double sum = 0;
                for (int col = row + 1; col < augmented.Columns - 1; col++)
                    sum += augmented[row, col] * eigenvector[col];

                if (Math.Abs(augmented[row, row]) > 1e-10)
                    eigenvector[row] = -sum / augmented[row, row];
                else
                    eigenvector[row] = 1; 
            }

            double norm = Math.Sqrt(eigenvector.Sum(x => x * x));
            for (int i = 0; i < eigenvector.Length; i++)
                eigenvector[i] /= norm;

            return eigenvector;
        }

        private double PowerMethod(Matrix matrix, int maxIterations = 100, double tolerance = 1e-10)
        {
            Vector b = new Vector(new double[matrix.Rows]);
            for (int i = 0; i < b.Dimension; i++) b.Components[i] = 1.0;

            double eigenvalue = 0;
            for (int i = 0; i < maxIterations; i++)
            {
                Vector newB = MatrixVectorMultiply(matrix, b);
                double newEigenvalue = newB.Components[0] / b.Components[0];

                if (Math.Abs(newEigenvalue - eigenvalue) < tolerance)
                    return newEigenvalue;

                eigenvalue = newEigenvalue;
                double norm = Math.Sqrt(newB.Components.Sum(x => x * x));
                for (int j = 0; j < b.Dimension; j++)
                    b.Components[j] = newB.Components[j] / norm;
            }
            return eigenvalue;
        }

        private Vector MatrixVectorMultiply(Matrix matrix, Vector vector)
        {
            if (matrix.Columns != vector.Dimension)
                throw new ArgumentException("Количество столбцов матрицы должно совпадать с размерностью вектора");

            double[] result = new double[matrix.Rows];

            for (int i = 0; i < matrix.Rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < matrix.Columns; j++)
                {
                    sum += matrix[i, j] * vector.Components[j];
                }
                result[i] = sum;
            }

            return new Vector(result);
        }

        private Matrix MatrixSubtract(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new ArgumentException("Матрицы должны быть одинакового размера");

            double[,] result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return new Matrix(result);
        }

        private Matrix MatrixIdentity(int size)
        {
            double[,] identity = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                identity[i, i] = 1.0;
            }
            return new Matrix(identity);
        }
    }
}
