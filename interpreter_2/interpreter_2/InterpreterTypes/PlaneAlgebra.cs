using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter_2.InterpreterTypes
{
    public sealed class PlaneAlgebra
    {
        public PlaneAlgebra() { }

        #region strings
        public double[] LinesIntersection(double A1, double B1, double C1, double A2, double B2, double C2)
        {
            double det = A1 * B2 - A2 * B1;
            double[] dot = new double[2];

            if (Math.Abs(det) < 1e-10)
            {
                throw new ArgumentException("прямые паралелльны или совпадают");
            }
            else
            {
                dot[0] = (B1 * C2 - B2 * C1) / det;
                dot[1] = (A2 * C1 - A1 * C2) / det;
            }
            
            return dot;
        }

        public double PointToLineDistance(Vector linePoint, Vector lineDirection, Vector point)
        {
            VectorAlgebra _v = new VectorAlgebra();

            Vector P0P = _v.VectorArithmetic("-", [point, linePoint]);
            double distance = _v.VectorMagnitude(P0P.Cross(lineDirection)) / _v.VectorMagnitude(lineDirection);
            return distance;
        }

        public Vector SymmetricPoint(Vector linePoint, Vector lineDirection, Vector point)
        {
            VectorAlgebra _v = new VectorAlgebra(); 
            Vector P0P = _v.VectorArithmetic("-", [point, linePoint]);
            double t = _v.DotProduct([P0P, lineDirection]) / _v.DotProduct([lineDirection, lineDirection]);
            Vector projection = _v.VectorArithmetic("+", [linePoint, lineDirection.Multiply(t)]);

            Vector symmetric = _v.VectorArithmetic("-", [projection.Multiply(2), point]);
            return symmetric;
        }



        #endregion







        #region notstrings

        public string LineEquations2DToString(double A, double B, double C)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Исходное уравнение прямой: }");
            sb.AppendLine($@"{A:0.###}x + {B:0.###}y + {C:0.###} = 0\\");

            if (Math.Abs(B) > 1e-10)
            {
                double k = -A / B;
                double b = -C / B;
                sb.AppendLine(@"\text{Уравнение с угловым коэффициентом: }");
                sb.AppendLine($@"y = {k:0.###}x {b:0.###;+0.###;-0.###}\\");
            }

            if (Math.Abs(A) > 1e-10 || Math.Abs(B) > 1e-10)
            {
                sb.AppendLine(@"\text{Каноническое уравнение: }");
                sb.AppendLine($@"\frac{{x}}{{{B:0.###}}} = \frac{{y}}{{{-A:0.###}}} = t\\");
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string LinesIntersectionToString(double A1, double B1, double C1, double A2, double B2, double C2)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Уравнения прямых: }");
            sb.AppendLine($@"1: {A1:0.###}x + {B1:0.###}y + {C1:0.###} = 0\\");
            sb.AppendLine($@"2: {A2:0.###}x + {B2:0.###}y + {C2:0.###} = 0\\");

            double det = A1 * B2 - A2 * B1;

            if (Math.Abs(det) < 1e-10)
            {
                sb.AppendLine(@"\text{Прямые параллельны или совпадают}\\");
            }
            else
            {
                double x = (B1 * C2 - B2 * C1) / det;
                double y = (A2 * C1 - A1 * C2) / det;
                sb.AppendLine(@"\text{Точка пересечения: }");
                sb.AppendLine($@"\left({x:0.###}, {y:0.###}\right)\\");
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string PointToLineDistanceToString(Vector linePoint, Vector lineDirection, Vector point)
        {
            VectorAlgebra _v = new VectorAlgebra();
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Прямая задана точкой } \mathbf{P}_0 = " + VectorToLatex(linePoint) + @" \text{ и направляющим вектором } \mathbf{v} = " + VectorToLatex(lineDirection) + @"\\");
            sb.AppendLine(@"\text{Точка } \mathbf{P} = " + VectorToLatex(point) + @"\\");

            Vector P0P = _v.VectorArithmetic("-", [point, linePoint]);
            double distance = _v.VectorMagnitude(P0P.Cross(lineDirection)) / _v.VectorMagnitude(lineDirection);

            sb.AppendLine(@"\text{Расстояние: }");
            sb.AppendLine($@"d = \frac{{|\mathbf{{P_0P}} \times \mathbf{{v}}|}}{{|\mathbf{{v}}|}} = {distance:0.###}\\");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string SymmetricPointToString(Vector linePoint, Vector lineDirection, Vector point)
        {
            VectorAlgebra _v = new VectorAlgebra();
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Исходная точка } \mathbf{P} = " + VectorToLatex(point) + @"\\");

            Vector P0P = _v.VectorArithmetic("-", [point, linePoint]);
            double t = _v.DotProduct([P0P, lineDirection]) / _v.DotProduct([lineDirection, lineDirection]);
            Vector projection = _v.VectorArithmetic("+", [linePoint, lineDirection.Multiply(t)]);

            Vector symmetric = _v.VectorArithmetic("-", [projection.Multiply(2), point]);

            sb.AppendLine(@"\text{Проекция точки: } \mathbf{P}' = " + VectorToLatex(projection) + @"\\");
            sb.AppendLine(@"\text{Симметричная точка: } \mathbf{P}'' = " + VectorToLatex(symmetric) + @"\\");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string PlaneEquationsToString(double A, double B, double C, double D)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Общее уравнение плоскости: }");
            sb.AppendLine($@"{A:0.###}x + {B:0.###}y + {C:0.###}z + {D:0.###} = 0\\");

            if (Math.Abs(A * B * C * D) > 1e-10)
            {
                sb.AppendLine(@"\text{Уравнение в отрезках: }");
                sb.AppendLine($@"\frac{{x}}{{{-D / A:0.###}}} + \frac{{y}}{{{-D / B:0.###}}} + \frac{{z}}{{{-D / C:0.###}}} = 1\\");
            }

            double norm = Math.Sqrt(A * A + B * B + C * C);
            sb.AppendLine(@"\text{Нормальное уравнение: }");
            sb.AppendLine($@"\frac{{{A:0.###}x + {B:0.###}y + {C:0.###}z + {D:0.###}}}{{{norm:0.###}}} = 0\\");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string LineEquationsNDToString(Vector point, Vector direction)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Параметрические уравнения прямой: }\\");

            for (int i = 0; i < point.Dimension; i++)
            {
                sb.AppendLine($@"x_{i + 1} = {point.Components[i]:0.###} + {direction.Components[i]:0.###}t\\");
            }

            if (point.Dimension == 3)
            {
                sb.AppendLine(@"\text{Канонические уравнения: }");
                sb.AppendLine($@"\frac{{x - {point.Components[0]:0.###}}}{{{direction.Components[0]:0.###}}} = " +
                             $@"\frac{{y - {point.Components[1]:0.###}}}{{{direction.Components[1]:0.###}}} = " +
                             $@"\frac{{z - {point.Components[2]:0.###}}}{{{direction.Components[2]:0.###}}}\\");
            }

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string PlanesIntersectionToString(double A1, double B1, double C1, double D1,
                                        double A2, double B2, double C2, double D2)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Уравнения плоскостей: }");
            sb.AppendLine($@"1: {A1:0.###}x + {B1:0.###}y + {C1:0.###}z + {D1:0.###} = 0\\");
            sb.AppendLine($@"2: {A2:0.###}x + {B2:0.###}y + {C2:0.###}z + {D2:0.###} = 0\\");

            Vector direction = new Vector(new double[] {B1*C2 - B2*C1, A2*C1 - A1*C2, A1*B2 - A2*B1});

            double x0 = 0, y0 = 0, z0 = 0;
            double det = A1 * B2 - A2 * B1;

            if (Math.Abs(det) > 1e-10)
            {
                x0 = (B1 * D2 - B2 * D1) / det;
                y0 = (A2 * D1 - A1 * D2) / det;
            }

            sb.AppendLine(@"\text{Прямая пересечения: }");
            sb.AppendLine(@"\text{Параметрические уравнения: }");
            sb.AppendLine($@"x = {x0:0.###} + {direction.Components[0]:0.###}t\\");
            sb.AppendLine($@"y = {y0:0.###} + {direction.Components[1]:0.###}t\\");
            sb.AppendLine($@"z = {z0:0.###} + {direction.Components[2]:0.###}t\\");

            sb.AppendLine(@"\text{Канонические уравнения: }");
            sb.AppendLine($@"\frac{{x - {x0:0.###}}}{{{direction.Components[0]:0.###}}} = " +
                         $@"\frac{{y - {y0:0.###}}}{{{direction.Components[1]:0.###}}} = " +
                         $@"\frac{{z - {z0:0.###}}}{{{direction.Components[2]:0.###}}}\\");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        public string LineProjectionOnPlaneToString(double planeA, double planeB, double planeC, double planeD,
                                                   Vector linePoint, Vector lineDirection)
        {
            VectorAlgebra _v = new VectorAlgebra();
            var sb = new StringBuilder();
            sb.AppendLine(@"\begin{align*}");
            sb.AppendLine(@"\text{Уравнение плоскости: }");
            sb.AppendLine($@"{planeA:0.###}x + {planeB:0.###}y + {planeC:0.###}z + {planeD:0.###} = 0\\");

            sb.AppendLine(@"\text{Исходная прямая: }");
            sb.AppendLine($@"\mathbf{{r}} = " + VectorToLatex(linePoint) + " + t" + VectorToLatex(lineDirection) + @"\\");

            Vector normal = new Vector(new double[] { planeA, planeB, planeC });
            double dot = _v.DotProduct([lineDirection, normal]);

            Vector projDirection = _v.VectorArithmetic("-", [lineDirection, normal.Multiply(dot / _v.DotProduct([normal, normal]))]);

            double t = -(planeA * linePoint.Components[0] + planeB * linePoint.Components[1] +
                        planeC * linePoint.Components[2] + planeD) / dot;
            Vector intersection = _v.VectorArithmetic("+", [linePoint, lineDirection.Multiply(t)]);

            sb.AppendLine(@"\text{Проекция прямой: }");
            sb.AppendLine($@"\mathbf{{r}} = " + VectorToLatex(intersection) + " + t" + VectorToLatex(projDirection) + @"\\");

            sb.AppendLine(@"\end{align*}");
            return sb.ToString();
        }

        // Вспомогательные методы для работы с векторами
        private string VectorToLatex(Vector v)
        {
            return @"\begin{pmatrix}" + string.Join(@" \\ ", v.Components.Select(x => x.ToString("0.###"))) + @"\end{pmatrix}";
        }
        #endregion
    }
}
