using NetCheckApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    public class Program
    {
        (List<Vector3D>, List<Thetra>) AddCube(Vector3D v, int n) {
            List<Vector3D> p = new List<Vector3D>(9)
{
new Vector3D(0,0,0),
new Vector3D(1,0,0),
new Vector3D(0,1,0),
new Vector3D(1,1,0),
new Vector3D(0,0,1),
new Vector3D(1,0,1),
new Vector3D(0,1,1),
new Vector3D(1,1,1),
new Vector3D(0.5,0.5,0.5)
};
            List<Thetra> t = new List<Thetra>(12) {
new Thetra(0,1,4,8,0),
new Thetra(1,4,5,8,0),
new Thetra(1,3,5,8,0),
new Thetra(3,5,7,8,0),
new Thetra(2,3,7,8,0),
new Thetra(2,6,7,8,0),
new Thetra(0,2,4,8,0),
new Thetra(2,4,6,8,0),
new Thetra(0,1,2,8,0),
new Thetra(1,2,3,8,0),
new Thetra(4,5,6,8,0),
new Thetra(5,6,7,8,0)
};
            return (p.Select(x => x + v).ToList(), t.Select(x=>new Thetra(x[0] + n, x[1] + n, x[2] + n, x[3] + n, x.mat)).ToList());
        }

        static void Main() {
            Func<int, double> m = (int mat) => {
                return 2;
            };

            Func<double, double, double, int, double> f = (double x, double y, double z, int mat) => {
                return 0 * m(mat);
            };

            Func<double, double, double, double> bound = (double x, double y, double z) => {
                return x + y + z;
            };
            List<int> borderPoints = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };

            MatrixGenerator c = new MatrixGenerator(p, t, f, m);
            double[,] koefs = c.CollectGlobalMatrix();
            c.AccountMainCondition(borderPoints, bound);
            c.Save();
            LOS L = new LOS();
            L.Load();
            L.FactorLU();
            L.Solve();
            L.Save("ans.txt");
        }
    }
}
