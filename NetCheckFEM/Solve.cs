using NetCheckApp;
using System;
using System.Collections.Generic;
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

        public Solve(List<Vector3D> _points, List<Thetra> _thetras, double[,] _coefficients, double[] _q)
        {
            q = _q;
            points = _points;
            thetras = _thetras;
            coefficients = _coefficients;
        }

        bool IsContains(Vector3D p, Thetra t)
        {
            return Vector3D.Cross(points[t[1]] - points[t[0]], points[t[2]] - points[t[0]]) * (p - points[t[0]]) >= 0 &&
                Vector3D.Cross(points[t[3]] - points[t[1]], points[t[2]] - points[t[1]]) * (p - points[t[1]]) >= 0 &&
                Vector3D.Cross(points[t[1]] - points[t[0]], points[t[3]] - points[t[0]]) * (p - points[t[0]]) >= 0 &&
                Vector3D.Cross(points[t[2]] - points[t[0]], points[t[3]] - points[t[0]]) * (p - points[t[0]]) >= 0;
        }

        public double U(Vector3D p)
        {
            double ans = 0;
            foreach (Thetra t in thetras)
                if (IsContains(p, t))
                    for (int i = 0; i < 4; i++)
                        ans += (coefficients[t[i], 0] + coefficients[t[i], 1] * p.X + coefficients[t[i], 2] * p.Y + coefficients[t[i], 3] * p.Z) * q[i];
            return ans;
        }
    }
}
