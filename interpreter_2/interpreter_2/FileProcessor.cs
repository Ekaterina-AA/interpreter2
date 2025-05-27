using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using interpreter_2.InterpreterTypes;

namespace interpreter_2
{
    internal class FileProcessor
    {
        private string _inputFilePath;
        private string _outputTexPath;
        private string _outputPdfPath;
        private StringBuilder _texBuilder;

        public FileProcessor(string inputFilePath, string outputDirectory)
        {
            _inputFilePath = inputFilePath;
            _outputTexPath = Path.Combine(outputDirectory, "solution.tex");
            _outputPdfPath = Path.Combine(outputDirectory, "solution.pdf");
            _texBuilder = new StringBuilder();
        }

        public void ProcessFile()
        {
            string latexSolution;
            var lines = File.ReadAllLines(_inputFilePath);
            _texBuilder.AppendLine(@"\documentclass{article}");
            _texBuilder.AppendLine(@"\usepackage[utf8]{inputenc}");
            _texBuilder.AppendLine(@"\usepackage[russian]{babel}");
            _texBuilder.AppendLine(@"\usepackage{amsmath}");
            _texBuilder.AppendLine(@"\usepackage{amssymb}");
            _texBuilder.AppendLine(@"\begin{document}");
            _texBuilder.AppendLine(@"\title{Решение задач}");
            _texBuilder.AppendLine(@"\maketitle");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length != 2) continue;

                var operation = parts[0].Trim();
                var data = parts[1].Trim();

                _texBuilder.AppendLine($@"\section{{Задача: {operation}}}");
                _texBuilder.AppendLine($@"\subsection{{Исходные данные}}");
                _texBuilder.AppendLine(data.Replace(",", ", "));

                _texBuilder.AppendLine($@"\subsection{{Решение}}");

                try
                {
                    latexSolution = ProcessOperation(operation, data);
                    _texBuilder.Append(latexSolution);
                }
                catch (Exception ex)
                {
                    _texBuilder.AppendLine($@"\textcolor{{red}}{{Ошибка при обработке операции: {ex.Message}}}");
                }
            }

            _texBuilder.AppendLine(@"\end{document}");
            File.WriteAllText(_outputTexPath, _texBuilder.ToString()); 

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pdflatex", 
                    Arguments = $"-interaction=nonstopmode -output-directory=\"{Path.GetDirectoryName(_outputTexPath)}\" \"{_outputTexPath}\"",
                    WorkingDirectory = Path.GetDirectoryName(_outputTexPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Ошибка компиляции LaTeX)");
                    }

                    process.Start();
                    process.WaitForExit();
                }

                var tempExtensions = new[] { ".aux", ".log", ".out", ".toc" };
                foreach (var ext in tempExtensions)
                {
                    var tempFile = Path.ChangeExtension(_outputTexPath, ext);
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }

                string pdfPath = Path.ChangeExtension(_outputTexPath, ".pdf");
                if (!File.Exists(pdfPath))
                {
                    throw new Exception("PDF не был создан (неизвестная ошибка)");
                }

                Console.WriteLine($"PDF успешно сгенерирован: {pdfPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось скомпилировать PDF.\nОшибка: {ex.Message}");
            }
        }

        private string ProcessOperation(string operation, string data)
        {
            VectorAlgebra _v = new VectorAlgebra();
            MatrixAlgebra _m = new MatrixAlgebra();
            PlaneAlgebra  _p = new PlaneAlgebra();
            Parser _parser = new Parser();
            Vector[] vectors;
            Matrix m1, m2;
            string latexSolution;
            switch (operation)
            {
                case "Скалярное произведение":
                    vectors = _parser.ParseVectors(data, 2);
                    latexSolution = _v.DotProductToString(vectors);
                    break;
                case "Векторное произведение":
                    vectors = _parser.ParseVectors(data, 2);
                    latexSolution = _v.CrossProductToString(vectors);
                    break;
                case "Смешанное произведение":
                    vectors = _parser.ParseVectors(data, 3);
                    latexSolution = _v.TripleProductToString(vectors);
                    break;
                case "Арифметические операции над векторами":
                    var parts_t = data.Split(new[] { ':' }, 3);
                    if (parts_t.Length != 3) 
                        throw new ArgumentException("Нужен формат 'Арифметические операции над векторами:операция между векторами: количество векторов: вектор а; вектор в... '");

                    var operation_t = parts_t[0].Trim();
                    int.TryParse(parts_t[1].Trim(), out int n);
                    var data_t = parts_t[2].Trim();

                    vectors = _parser.ParseVectors(data_t, n);
                    latexSolution = _v.VectorArithmeticToString(operation_t, vectors);
                    break;
                case "Модуль вектора":
                    vectors = _parser.ParseVectors(data, 1);
                    latexSolution = _v.VectorMagnitudeToString(vectors[0]);
                    break;
                case "Процесс ортогонализации":
                    var parts_t2 = data.Split(new[] { ':' }, 2);
                    if (parts_t2.Length != 2) 
                        throw new ArgumentException("Нужен формат 'Процесс ортогонализации: количество векторов: вектор а; вектор в... '");
                    
                    int.TryParse(parts_t2[0], out int n2);

                    vectors = _parser.ParseVectors(parts_t2[1], n2);
                    latexSolution = _v.OrthogonalizationToString(vectors);
                    break;
                case "Арифметические операции над матрицами":
                    var parts_t3 = data.Split(new[] { ':' }, 2);
                    var parts_t9 = parts_t3[1].Split(new[] { ';' }, 2);
                    if (int.TryParse(parts_t9[1].Trim(), out int n3))
                    {
                        m1 = _parser.ParseMatrix(parts_t9[0]);
                        latexSolution = _m.MatrixArithmeticToString(parts_t3[0], m1, b: default,  n3);
                    }
                    else
                    {
                        m1 = _parser.ParseMatrix(parts_t9[0]);
                        m2 = _parser.ParseMatrix(parts_t9[1]);

                        latexSolution = _m.MatrixArithmeticToString(parts_t3[0], m1, m2);
                    }
                    break;
                case "Определитель матрицы":
                    m1 = _parser.ParseMatrix(data);
                    latexSolution = _m.MatrixDeterminantToString(m1);
                    break;
               case "Обратная матрица":
                    m1 = _parser.ParseMatrix(data);
                    latexSolution = _m.InverseMatrixToString(m1); 
                    break;
                case "Решение СЛАУ":
                    var parts_t7 = data.Split(new[] { ':' }, 2);
                    var parts_t8  = parts_t7[1].Split(new[] { ';' }, 2);
                    m1 = _parser.ParseMatrix(parts_t8[0]);
                    m2 = _parser.ParseMatrix(parts_t8[1]);

                    latexSolution = _m.SolveLinearSystemToString(m1, m2, parts_t7[0]);
                    break;
                case "Собственные числа":
                    m1 = _parser.ParseMatrix(data);
                    latexSolution = _m.EigenvaluesToString(m1);
                    break;
                case "Собственные векторы":
                    m1 = _parser.ParseMatrix(data);
                    latexSolution = _m.EigenvectorsToString(m1);
                    break;
                case "Ранг матрицы":
                    m1 = _parser.ParseMatrix(data);
                    latexSolution = _m.MatrixRankToString(m1);
                    break;
                case "Размер линейной оболочки":
                    var parts_t4 = data.Split(new[] { ':' }, 2);
                    int.TryParse(parts_t4[0].Trim(), out int n4);
                    vectors = _parser.ParseVectors(parts_t4[1], n4);
                    latexSolution = _m.LinearSpanDimensionToString(vectors);
                    break;
                case "Принадлежность линейной оболочке":
                    var parts_t5 = data.Split(new[] { ':' }, 2);
                    var parts_t6 = parts_t5[1].Split(new[] { ';' }, 2);
                    int.TryParse(parts_t5[0].Trim(), out int n5);
                    var v1 = _parser.ParseVectors(parts_t6[0], 1);
                    vectors = _parser.ParseVectors(parts_t6[1], n5);
                    latexSolution = _m.CheckLinearSpanMembershipToString(v1[0], vectors);
                    break;
                case "Уравнения прямой на плоскости":
                    var parts_t10 = data.Split(new[] { ';' }, 3);
                    double.TryParse(parts_t10[0].Trim(), out double n6);
                    double.TryParse(parts_t10[1].Trim(), out double n7);
                    double.TryParse(parts_t10[2].Trim(), out double n8);
                    latexSolution = _p.LineEquations2DToString(n6, n7, n8);
                    break;
                case "Точка пересечения прямых":
                    var parts_t11 = data.Split(new[] { ';' }, 6);
                    latexSolution = _p.LinesIntersectionToString(
                        double.Parse(parts_t11[0].Trim()),
                        double.Parse(parts_t11[1].Trim()),
                        double.Parse(parts_t11[2].Trim()),
                        double.Parse(parts_t11[3].Trim()),
                        double.Parse(parts_t11[4].Trim()),
                        double.Parse(parts_t11[5].Trim()));
                    break;
                case "Расстояние от точки до прямой":
                    vectors = _parser.ParseVectors(data, 3);
                    latexSolution = _p.PointToLineDistanceToString(vectors[0], vectors[1], vectors[2]);
                    break;
                case "Симметричная точка относительно прямой":
                    vectors = _parser.ParseVectors(data, 3);
                    latexSolution = _p.SymmetricPointToString(vectors[0], vectors[1], vectors[2]);
                    break;
                case "Уравнения плоскости":
                    var parts_t12 = data.Split(new[] { ';' }, 4);
                    latexSolution = _p.PlaneEquationsToString(
                        double.Parse(parts_t12[0].Trim()),
                        double.Parse(parts_t12[1].Trim()),
                        double.Parse(parts_t12[2].Trim()),
                        double.Parse(parts_t12[3].Trim()));
                    break;
                case "Уравнения прямой в n-мерном пространстве":
                    vectors = _parser.ParseVectors(data, 2);
                    latexSolution = _p.LineEquationsNDToString(vectors[0], vectors[1]);
                    break;
                case "Пересечение плоскостей":
                    var parts_t13 = data.Split(new[] { ';' }, 8);
                    latexSolution = _p.PlanesIntersectionToString(
                        double.Parse(parts_t13[0].Trim()),
                        double.Parse(parts_t13[1].Trim()),
                        double.Parse(parts_t13[2].Trim()),
                        double.Parse(parts_t13[3].Trim()),
                        double.Parse(parts_t13[4].Trim()),
                        double.Parse(parts_t13[5].Trim()),
                        double.Parse(parts_t13[6].Trim()),
                        double.Parse(parts_t13[7].Trim())
                        );
                    break;
                case "Проекция прямой на плоскость":
                    var parts_t14 = data.Split(new[] { ';' }, 5);
                    vectors = _parser.ParseVectors(parts_t14[4], 2);
                    latexSolution = _p.LineProjectionOnPlaneToString(
                        double.Parse(parts_t14[0].Trim()),
                        double.Parse(parts_t14[1].Trim()),
                        double.Parse(parts_t14[2].Trim()),
                        double.Parse(parts_t14[3].Trim()),
                        vectors[0],
                        vectors[1]);
                    break;
                default:
                    throw new ArgumentException($"Неизвестная операция: {operation}");
            }
            
            return latexSolution;
        }
    }
}
