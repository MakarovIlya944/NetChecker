using NetCheckApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    public class Program
    {
        static void Main()
        {
            List<Vector3D> b = new List<Vector3D>(9)
            {
new Vector3D(1,0,0),
new Vector3D(2,1,0),
new Vector3D(1,1,0),
new Vector3D(2,2,0),
new Vector3D(0,0,3),
new Vector3D(1,1,3),
new Vector3D(0,1,3),
new Vector3D(1,2,3),
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

            Func<int, double> m = (int mat) =>
            {
                return 3;
            };

            Func<double, double, double, int, double> f = (double x, double y, double z, int mat)=>
            {
                return -6*m(mat);
            };

            Func<double, double, double, double> bound = (double x, double y, double z) =>
            {
                return x*x+y*y+z*z;
            };

            //NetGenerator n = new NetGenerator("ok.txt");
            
            MatrixGenerator c = new MatrixGenerator(b, t, f, m);
            c.CollectGlobalMatrix();
            c.AccountMainCondition(Enumerable.Range(0,8).ToList(), bound);
            c.Save();
            LOS L = new LOS();
            L.Load();
            L.FactorLU();
            L.Solve();
            L.Save("ans.txt");
        }
    }
}
