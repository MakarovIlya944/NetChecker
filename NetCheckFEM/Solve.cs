using NetCheckApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    class Solve
    {
        List<Vector3D> points;
        List<Thetra> thetras;
        double[,] coefficients;
        double[] q;
        Func<double, double, double, double> exact;
        Vector3D max, min;
        double step = 0.5;

        public Solve(List<Vector3D> _points, List<Thetra> _thetras, double[,] _coefficients, double[] _q, Func<double, double, double, double> _exact)
        {
            q = _q;
            points = _points;
            max = points[0];
            min = points[0];
            foreach (Vector3D item in points)
            {
                max = Vector3D.Max(item, max);
                min = Vector3D.Min(item, min);
            }
            thetras = _thetras;
            coefficients = _coefficients;
            exact = _exact;
        }

        bool IsContains(Vector3D p, Thetra t)
        {
            Vector3D a, b, c;
            bool ans = true;
            int[,] indexes = new int[4, 4]
            {
                { 1,2,0,3},
                { 3,2,1,0},
                { 1,3,0,2},
                { 2,3,0,1}
            };

            for(int i=0;i<4 && ans;i++)
            {
                a = Vector3D.Cross(points[t[indexes[i,0]]] - points[t[indexes[i, 2]]], points[t[indexes[i, 1]]] - points[t[indexes[i, 2]]]);
                b = points[t[indexes[i, 3]]] - (points[t[indexes[i, 0]]] + points[t[indexes[i, 1]]] + points[t[indexes[i, 2]]])/3.0;
                c = p - (points[t[indexes[i, 0]]] + points[t[indexes[i, 1]]] + points[t[indexes[i, 2]]])/3.0;
                ans &= !((a * b >= 0) ^ (a * c >= 0));
            }

            return ans || t.data.Select(x => points[x]).Contains(p);
        }

        public double U(Vector3D p)
        {
            double ans = 0;
            foreach (Thetra t in thetras)
                if (IsContains(p, t))
                {
                    for (int i = 0; i < 4; i++)
                        ans += (coefficients[t[i], 0] + coefficients[t[i], 1] * p.X + coefficients[t[i], 2] * p.Y + coefficients[t[i], 3] * p.Z) * q[i];
                    break;
                }
            return ans;
        }

        public void PrintAbsDiff()
        {
            int i = 0;
            foreach (Vector3D item in points)
                Console.WriteLine("#{0}\t{1:#.####E+00}   \t{2:#.####E+00}   \t{3:#.####E+00}",
                    i, q[i], exact(item.X, item.Y, item.Z), Math.Abs(q[i++] - exact(item.X, item.Y, item.Z))
                    );
        }

        public void Resolve()
        {
            StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resolve.txt"));
            double stepX = (max.X-min.X)*step, stepY = (max.Y - min.Y) * step, stepZ = (max.Z - min.Z) * step;
            int n = (int)(1 / step)+1;
            string top = "\t" + Enumerable.Range(0, n).Select(x => (x * stepY).ToString("000.0000\t")).Aggregate((x, y) => x += y);


            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"{i * stepZ:0.00e+00}" + top);
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{j * stepY:0.00e+00}\t");
                    for (int k = 0; k < n; k++)
                        Console.Write($"{U(min + new Vector3D(k * stepX, j * stepY, i * stepZ)):0.00e+00}\t");
                    Console.WriteLine();
                }
                Console.WriteLine("---------------------------------------------------------------------------");
            }
            sw.Close();
        }
    }
}
