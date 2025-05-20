using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace interpreter_2.InterpreterTypes
{
    public sealed class Vector
    {
        public double[] Components { get; set; }

        public Vector(double[] components)
        {
            Components = components;
        }


        public int Dimension => Components.Length;

        public override string ToString()
        {
            return $"({string.Join(", ", Components)})";
        }
    }

    
    public sealed class VectorAlgebra
    {
        Parser parser;
        public VectorAlgebra() { parser = new Parser(); }
        
        #region notstrings

        public double DotProduct(Vector[] vectors)
        {
            if (vectors[0].Dimension != vectors[1].Dimension)
                throw new ArgumentException("Векторы должны быть одинаковой размерности");

            double result = 0;

            for (int i = 0; i < vectors[0].Dimension; i++)
            {
                result += vectors[0].Components[i] * vectors[1].Components[i];
            }

            return result;
        }

        public double[] CrossProduct(Vector[] vectors)
        {
            double[] result = new double[3];

            if (vectors[0].Dimension != vectors[1].Dimension || (vectors[0].Dimension != 3 && vectors[0].Dimension != 7))
                throw new ArgumentException("Векторное произведение определено только для 3D и 7D векторов");

            if (vectors[0].Dimension == 3)
            {
                var a = vectors[0];
                var b = vectors[1];
                result[0] = a.Components[1] * b.Components[2] - a.Components[2] * b.Components[1];
                result[1] = a.Components[2] * b.Components[0] - a.Components[0] * b.Components[2];
                result[2] = a.Components[0] * b.Components[1] - a.Components[1] * b.Components[0];
            }
            else
            {
                // Реализация для 7D 

            }
            return result;
        }

        public double TripleProduct(Vector[] vectors)
        {
            var sb = new StringBuilder();
            double result = 0.0;

            if (vectors.Length != 3 || (vectors[0].Dimension != 3 && vectors[0].Dimension != 7))
                throw new ArgumentException("Смешанное произведение определено для 3 векторов в 3D или 7D");

            var a = vectors[0];
            var b = vectors[1];
            var c = vectors[2];

            if (a.Dimension == 3)
            {
                double[] cross = new double[3] 
                {
                    a.Components[1] * b.Components[2] - a.Components[2] * b.Components[1],
                    a.Components[2] * b.Components[0] - a.Components[0] * b.Components[2],
                    a.Components[0] * b.Components[1] - a.Components[1] * b.Components[0]
                };

                result = cross[0] * c.Components[0] + cross[1] * c.Components[1] + cross[2] * c.Components[2];
            }
            else
            {
                // 7D реализация
            }

            return result;
        }

        public Vector VectorArithmetic(string operation, Vector[] vectors)
        {
            var sb = new StringBuilder();

            if (vectors.Length < 2)
                throw new ArgumentException("Нужно минимум 2 вектора");

            Vector result = operation switch
            {
                "+" => new Vector(vectors[0].Components.Zip(vectors[1].Components, (a, b) => a + b).ToArray()),
                "-" => new Vector(vectors[0].Components.Zip(vectors[1].Components, (a, b) => a - b).ToArray()),
                "*" => new Vector(vectors[0].Components.Select(x => x * vectors[1].Components[0]).ToArray()),
                _ => throw new ArgumentException("Неподдерживаемая операция")
            };

            return result;
        }

        public double VectorMagnitude(Vector vector)
        {
            double sumOfSquares = vector.Components.Sum(x => x * x);
            double magnitude = Math.Sqrt(sumOfSquares);
            return magnitude;
        }

        public Vector[] OrthogonalizationT(Vector[] vectors)
        {
            Vector[] orthoVectors = new Vector[vectors.Length];

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector u = vectors[i];
                for (int j = 0; j < i; j++)
                {
                    double dot = vectors[i].Components.Zip(orthoVectors[j].Components, (a, b) => a * b).Sum();
                    double norm = orthoVectors[j].Components.Sum(x => x * x);
                    double coef = dot / norm;

                    u = new Vector(u.Components.Zip(orthoVectors[j].Components, (a, b) => a - coef * b).ToArray());
                }
                orthoVectors[i] = u;
            }
            return orthoVectors;
        }
        #endregion








        #region strings
        public string DotProductToString(Vector[] vectors)
        {
            if (vectors[0].Dimension != vectors[1].Dimension)
                throw new ArgumentException("Векторы должны быть одинаковой размерности");

            double result = 0;
            var sb = new StringBuilder();

            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\mathbf{a} \cdot \mathbf{b} &= ");

            for (int i = 0; i < vectors[0].Dimension; i++)
            {
                result += vectors[0].Components[i] * vectors[1].Components[i];
                sb.Append($@"{vectors[0].Components[i]} \times {vectors[1].Components[i]}");

                if (i < vectors[0].Dimension - 1)
                {
                    sb.AppendLine(@" + \\ &+ ");
                }
            }

            sb.AppendLine(@" = \\");
            sb.AppendLine($@"&= {string.Join(" + ", vectors[0].Components.Select((v, i) => $"{v * vectors[1].Components[i]}"))} \\");
            sb.AppendLine($@"&= {result}");
            sb.AppendLine(@"\end{align*}");

            return sb.ToString();
        }

        public string CrossProductToString(Vector[] vectors)
        {
            var sb = new StringBuilder();

            if (vectors[0].Dimension != vectors[1].Dimension || (vectors[0].Dimension != 3 && vectors[0].Dimension != 7))
                throw new ArgumentException("Векторное произведение определено только для 3D и 7D векторов");

            if (vectors[0].Dimension == 3)
            {
                var a = vectors[0];
                var b = vectors[1];
                double[] result = new double[3];
                result[0] = a.Components[1] * b.Components[2] - a.Components[2] * b.Components[1];
                result[1] = a.Components[2] * b.Components[0] - a.Components[0] * b.Components[2];
                result[2] = a.Components[0] * b.Components[1] - a.Components[1] * b.Components[0];

                sb.AppendLine(@"\begin{align*}");
                sb.AppendLine($@"\mathbf{{a}} \times \mathbf{{b}} &= \begin{{vmatrix}} \mathbf{{i}} & \mathbf{{j}} & \mathbf{{k}} \\ {a.Components[0]} & {a.Components[1]} & {a.Components[2]} \\ {b.Components[0]} & {b.Components[1]} & {b.Components[2]} \end{{vmatrix}} \\");
                sb.AppendLine($@"&= \mathbf{{i}}({a.Components[1]} \times {b.Components[2]} - {a.Components[2]} \times {b.Components[1]}) \\");
                sb.AppendLine($@"&- \mathbf{{j}}({a.Components[0]} \times {b.Components[2]} - {a.Components[2]} \times {b.Components[0]}) \\");
                sb.AppendLine($@"&+ \mathbf{{k}}({a.Components[0]} \times {b.Components[1]} - {a.Components[1]} \times {b.Components[0]}) \\");
                sb.AppendLine($@"&= ({result[0]}, {result[1]}, {result[2]})");
                sb.AppendLine(@"\end{align*}");
            }
            else
            {
                // Реализация для 7D 
                
            }
            return sb.ToString();
        }


        public string TripleProductToString(Vector[] vectors)
        {
            var sb = new StringBuilder();

            if (vectors.Length != 3 || (vectors[0].Dimension != 3 && vectors[0].Dimension != 7))
                throw new ArgumentException("Смешанное произведение определено для 3 векторов в 3D или 7D");

            var a = vectors[0];
            var b = vectors[1];
            var c = vectors[2];

            if (a.Dimension == 3)
            {
                double[] cross = new double[3] 
                {
                    a.Components[1] * b.Components[2] - a.Components[2] * b.Components[1],
                    a.Components[2] * b.Components[0] - a.Components[0] * b.Components[2],
                    a.Components[0] * b.Components[1] - a.Components[1] * b.Components[0]
                };

                double result = cross[0] * c.Components[0] + cross[1] * c.Components[1] + cross[2] * c.Components[2];

                sb.AppendLine(@"\begin{align*}");
                sb.AppendLine($@"[\mathbf{{a}}, \mathbf{{b}}, \mathbf{{c}}] &= (\mathbf{{a}} \times \mathbf{{b}}) \cdot \mathbf{{c}} \\");
                sb.AppendLine($@"&= \left(\begin{{vmatrix}} {a.Components[1]} & {a.Components[2]} \\ {b.Components[1]} & {b.Components[2]} \end{{vmatrix}}, -\begin{{vmatrix}} {a.Components[0]} & {a.Components[2]} \\ {b.Components[0]} & {b.Components[2]} \end{{vmatrix}}, \begin{{vmatrix}} {a.Components[0]} & {a.Components[1]} \\ {b.Components[0]} & {b.Components[1]} \end{{vmatrix}}\right) \cdot ({string.Join(", ", c.Components)}) \\");
                sb.AppendLine($@"&= ({cross[0]} \times {c.Components[0]}) + ({cross[1]} \times {c.Components[1]}) + ({cross[2]} \times {c.Components[2]}) \\");
                sb.AppendLine($@"&= {result}");
                sb.AppendLine(@"\end{align*}");
            }
            else
            {
                // 7D реализация
            }

            return sb.ToString();
        }

        public string VectorArithmeticToString(string operation, Vector[] vectors)
        {
            var sb = new StringBuilder();

            if (vectors.Length < 2)
                throw new ArgumentException("Нужно минимум 2 вектора");

            Vector result = operation switch
            {
                "+" => new Vector(vectors[0].Components.Zip(vectors[1].Components, (a, b) => a + b).ToArray()),
                "-" => new Vector(vectors[0].Components.Zip(vectors[1].Components, (a, b) => a - b).ToArray()),
                "*" => new Vector(vectors[0].Components.Select(x => x * vectors[1].Components[0]).ToArray()),
                _ => throw new ArgumentException("Неподдерживаемая операция")
            };

            sb.AppendLine(@"\begin{align*}");
            sb.Append($@"\mathbf{{v}}_1 {operation} \mathbf{{v}}_2 &= ");
            sb.Append($@"({string.Join(", ", vectors[0].Components)}) {operation} ({string.Join(", ", vectors[1].Components)}) \\");
            sb.AppendLine($@"&= ({string.Join(", ", result.Components)})");
            sb.AppendLine(@"\end{align*}");

            return sb.ToString();
        }

        public string VectorMagnitudeToString(Vector vector)
        {
            double sumOfSquares = vector.Components.Sum(x => x * x);
            double magnitude = Math.Sqrt(sumOfSquares);

            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine($@"\|\mathbf{{v}}\| &= \sqrt{{ {string.Join(" + ", vector.Components.Select(x => $"{x}^2"))} }} \\");
            sb.AppendLine($@"&= \sqrt{{ {sumOfSquares} }} \\");
            sb.AppendLine($@"&= {magnitude}");
            sb.AppendLine(@"\end{align*}");

            return sb.ToString();
        }

        public string OrthogonalizationToString(Vector[] vectors)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Процесс ортогонализации Грама-Шмидта:}\\");

            List<Vector> orthoVectors = new List<Vector>();
            for (int i = 0; i < vectors.Length; i++)
            {
                Vector u = vectors[i];
                for (int j = 0; j < i; j++)
                {
                    double dot = vectors[i].Components.Zip(orthoVectors[j].Components, (a, b) => a * b).Sum();
                    double norm = orthoVectors[j].Components.Sum(x => x * x);
                    double coef = dot / norm;

                    u = new Vector(u.Components.Zip(orthoVectors[j].Components, (a, b) => a - coef * b).ToArray());

                    sb.AppendLine($@"\mathbf{{u}}_{i + 1} &= \mathbf{{v}}_{i + 1} - \frac{{(\mathbf{{v}}_{i + 1} \cdot \mathbf{{u}}_{j + 1})}}{{(\mathbf{{u}}_{j + 1} \cdot \mathbf{{u}}_{j + 1})}} \mathbf{{u}}_{j + 1} \\");
                    sb.AppendLine($@"&= ({string.Join(", ", vectors[i].Components)}) - \frac{{{dot}}}{{{norm}}}({string.Join(", ", orthoVectors[j].Components)}) \\");
                }
                orthoVectors.Add(u);
                sb.AppendLine($@"\mathbf{{u}}_{i + 1} &= ({string.Join(", ", u.Components)}) \\");
            }

            sb.AppendLine(@"\text{Результат ортогонализации:}\\");
            for (int i = 0; i < orthoVectors.Count; i++)
            {
                sb.AppendLine($@"\mathbf{{u}}_{i + 1} = ({string.Join(", ", orthoVectors[i].Components)}) \\");
            }
            sb.AppendLine(@"\end{align*}");

            return sb.ToString();
        }
    }
    #endregion
}
