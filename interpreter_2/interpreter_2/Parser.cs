using interpreter_2.InterpreterTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter_2
{
    public sealed class Parser
    {
        public Parser() { }
        public Vector[] ParseVectors(string data, int expectedCount)
        {
            var parts = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != expectedCount)
                throw new ArgumentException($"Ожидается {expectedCount} вектора, получено {parts.Length}");

            var vectors = new Vector[expectedCount];
            for (int i = 0; i < expectedCount; i++)
            {
                var components = parts[i].Trim().Trim('(', ')').Split(',');
                var doubleComponents = components.Select(c => double.Parse(c.Trim())).ToArray();
                vectors[i] = new Vector(doubleComponents);
            }
            return vectors;
        }

        public Matrix ParseMatrix(string data)
        {
            string cleaned = data.Replace(" ", "").Trim('[', ']');

            string[] rows = cleaned.Split(new[] { "),(" }, StringSplitOptions.RemoveEmptyEntries);

            if (rows.Length > 0)
            {
                rows[0] = rows[0].TrimStart('(');
                rows[rows.Length - 1] = rows[rows.Length - 1].TrimEnd(')');
            }

            int rowCount = rows.Length;
            if (rowCount == 0) return new Matrix(new double[0, 0]);

            string[] firstCols = rows[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int colCount = firstCols.Length;

            var matrixData = new double[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                string[] cols = rows[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (cols.Length != colCount)
                    throw new ArgumentException("Все строки матрицы должны иметь одинаковое количество элементов");

                for (int j = 0; j < colCount; j++)
                {
                    if (!double.TryParse(cols[j], out matrixData[i, j]))
                        throw new ArgumentException($"Неверный формат числа: {cols[j]}");
                }
            }

            return new Matrix(matrixData);
        }
    }
}
