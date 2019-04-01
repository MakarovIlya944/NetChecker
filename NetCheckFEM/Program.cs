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
        static void Main()
        {
            List<Vector3D> p = new List<Vector3D>(14)
            {
new Vector3D(0,0,0),
new Vector3D(1,0,0),
new Vector3D(0,1,0),
new Vector3D(1,1,0),
new Vector3D(0,0,1),
new Vector3D(1,0,1),
new Vector3D(0,1,1),
new Vector3D(1,1,1),
new Vector3D(0.5,0.5,0.25),
new Vector3D(0.25,0.5,0.5),
new Vector3D(0.5,0.25,0.5),
new Vector3D(0.75,0.5,0.5),
new Vector3D(0.5,0.75,0.5),
new Vector3D(0.5,0.5,0.75)
            };

            List<Thetra> t = new List<Thetra>(24) {
new Thetra(9,10,12,13,0),
new Thetra(8,9,10,12,0),
new Thetra(8,10,11,13,0),
new Thetra(8,11,12,13,0),

new Thetra(0,1,5,10,0),
new Thetra(0,4,5,10,0),
new Thetra(1,3,5,11,0),
new Thetra(3,5,7,11,0),
new Thetra(2,3,6,12,0),
new Thetra(3,6,7,12,0),
new Thetra(0,2,6,9,0),
new Thetra(0,4,6,9,0),
new Thetra(0,1,3,8,0),
new Thetra(0,2,3,8,0),
new Thetra(4,5,6,13,0),
new Thetra(5,6,7,13,0),

new Thetra(0,4,9,10,0),
new Thetra(1,5,10,11,0),
new Thetra(3,7,11,12,0),
new Thetra(2,6,9,12,0),
new Thetra(0,1,8,10,0),
new Thetra(4,5,10,13,0),
new Thetra(6,7,12,13,0),
new Thetra(2,3,8,12,0)
            };

            Func<int, double> m = (int mat) =>
            {
                return 2;
            };

            Func<double, double, double, int, double> f = (double x, double y, double z, int mat)=>
            {
                return 0*m(mat);
            };

            Func<double, double, double, double> bound = (double x, double y, double z) =>
            {
                return x+y+z;
            };
            List<int> borderPoints = new List<int>() {0,1,2,3,4,5,6,7};

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
