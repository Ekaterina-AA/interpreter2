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
                    FileName = "cmd.exe",
                    Arguments = $"pdflatex {_outputTexPath}",
                    WorkingDirectory = Path.GetDirectoryName(_outputTexPath),
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
                
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"pdflatex {_outputTexPath}";
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_outputPdfPath);
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        var error = process.StandardError.ReadToEnd();
                        throw new Exception($"Ошибка компиляции LaTeX:\n{error}");
                    }
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

                Console.WriteLine($"PDF успешно сгенерирован: {Path.ChangeExtension(_outputTexPath, ".pdf")}");
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
                    var data_t = parts_t[0].Trim();

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
                    var parts_t3 = data.Split(new[] { ':' }, 3);
                    if (int.TryParse(parts_t3[2].Trim(), out int n3))
                    {
                        m1 = _parser.ParseMatrix(parts_t3[1]);
                        latexSolution = _m.MatrixArithmeticToString(parts_t3[0], m1, b: default,  n3);
                    }
                    else
                    {
                        m1 = _parser.ParseMatrix(parts_t3[1]);
                        m2 = _parser.ParseMatrix(parts_t3[2]);

                        latexSolution = _m.MatrixArithmeticToString(parts_t3[0], m1, m2);
                    }
                    break;
                //case "Определитель матрицы":
                //    latexSolution = _m.MatrixDeterminant(data);
                //    break;
                //case "Обратная матрица":
                //    latexSolution = _m.InverseMatrix(data);
                //    break;
                //case "Решение СЛАУ":
                //    latexSolution = _m.SolveLinearSystem(data);
                //    break;
                //case "Собственные числа":
                //    latexSolution = _m.Eigenvalues(data);
                //    break;
                //case "Собственные векторы":
                //    latexSolution = _m.Eigenvectors(data);
                //    break;
                //case "Ранг матрицы":
                //    latexSolution = _m.MatrixRank(data);
                //    break;
                //case "Размер линейной оболочки":
                //    latexSolution = _m.LinearSpanDimension(data);
                //    break;
                //case "Принадлежность линейной оболочке":
                //    latexSolution = _m.CheckLinearSpanMembership(data);
                //    break;
                //case "Уравнения прямой на плоскости":
                //    latexSolution = _p.LineEquations2D(data);
                //    break;
                //case "Точка пересечения прямых":
                //    latexSolution = _p.LinesIntersection2D(data);
                //    break;
                //case "Расстояние от точки до прямой":
                //    latexSolution = _p.PointToLineDistance(data);
                //    break;
                //case "Симметричная точка относительно прямой":
                //    latexSolution = _p.SymmetricPointAboutLine(data);
                //    break;
                //case "Уравнения плоскости":
                //    latexSolution = _p.PlaneEquations3D(data);
                //    break;
                //case "Уравнения прямой в n-мерном пространстве":
                //    latexSolution = _p.LineEquationsND(data);
                //    break;
                //case "Пересечение плоскостей":
                //    latexSolution = _p.PlanesIntersection(data);
                //    break;
                //case "Проекция прямой на плоскость":
                //    latexSolution = _p.LineProjectionOnPlane(data);
                //    break;
                default:
                    throw new ArgumentException($"Неизвестная операция: {operation}");
            }
            return latexSolution;
        }
    }
}
