using System;
using System.Collections.Generic;
using System.Linq;
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                sb.Append("(");
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(Data[i, j]);
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
                        // TODO: fix
                        //sb.AppendLine($@"{scalar} \times \mathbf{A} = ");
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
            // Реализация метода присоединённой матрицы
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\mathbf{A}^{-1} = \frac{1}{\det(\mathbf{A})} \cdot \text{adj}(\mathbf{A})");
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string SolveLinearSystemToString(Matrix coefficients, Matrix constants, string method)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");

            if (method == "Gauss")
            {
                sb.AppendLine(@"\text{Метод Гаусса:}\\");
                // Реализация прямого и обратного хода метода Гаусса
            }
            else if (method == "Jordan")
            {
                sb.AppendLine(@"\text{Метод Жордана-Гаусса:}\\");
                // Реализация метода Жордана-Гаусса
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string EigenvaluesToString(Matrix matrix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Собственные числа матрицы } \mathbf{A}:\\");
            // Реализация поиска собственных чисел
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string EigenvectorsToString(Matrix matrix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Собственные векторы матрицы } \mathbf{A}:\\");
            // Реализация поиска собственных векторов
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string MatrixRankToString(Matrix matrix)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Ранг матрицы } \mathbf{A} = " + CalculateRank(matrix));
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string LinearSpanDimensionToString(Vector[] vectors)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            // TODO: fix
            //sb.AppendLine(@"\text{Размер линейной оболочки} = " + CalculateRank(vectors));
            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string CheckLinearSpanMembershipToString(Vector vector, Vector[] span)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            bool belongs = CheckSpanMembership(vector, span);
            sb.AppendLine($@"\text{"Вектор"} ({string.Join(", ", vector.Components)}) {(belongs ? @"\in" : @"\notin")} \text{"линейной оболочки"}");
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

        private int CalculateRank(Matrix matrix)
        {
            // Реализация вычисления ранга через приведение к ступенчатому виду
            return 0;
        }

        private bool CheckSpanMembership(Vector vector, Vector[] span)
        {
            // Реализация проверки принадлежности линейной оболочке
            return false;
        }

        //public string MatrixArithmetic(string data) { /* ... */ }
        //public string MatrixDeterminant(string data) { /* ... */ }
        //public string InverseMatrix(string data) { /* ... */ }
        //
        //public string SolveLinearSystem(string data) { /* ... */ }
        //public string Eigenvalues(string data) { /* ... */ }
        //public string Eigenvectors(string data) { /* ... */ }
        //
        //public string MatrixRank(string data) { /* ... */ }
        //public string LinearSpanDimension(string data) { /* ... */ }
        //public string CheckLinearSpanMembership(string data) { /* ... */ }
    }
}
