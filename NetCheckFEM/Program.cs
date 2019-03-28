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
            List<Vector3D> b = new List<Vector3D>(5)
            {
               new Vector3D ( 1,0,0),
               new Vector3D ( 2,1,0),
               new Vector3D ( 2,-0.5,0),
               new Vector3D ( 1.5,-1,0),
               new Vector3D ( 0.5,-1,0),
               new Vector3D ( 0,-0.5,0),
               new Vector3D ( 1,0,1)
            };
            List<Thetra> t = new List<Thetra>(5) {
                new Thetra( 0,1,2,6,1),
                new Thetra( 0,2,3,6,1),
                new Thetra( 0,3,4,6,1),
                new Thetra( 0,4,5,6,1),
                new Thetra( 0,1,5,6,1)
            };

            Func<int, double> m = (int mat) =>
            {
                return 3;
            };

            Func<double, double, double, int, double> f = (double x, double y, double z, int mat)=>
            {
                return -6*m(mat);
            };

            MatrixGenerator c = new MatrixGenerator(b, t, f, m);
            //c.CollectGlobalMatrix();
            //c.Save();
            LOS L = new LOS();
            L.Load();
            L.FactorLU();
            L.SolveLU();
            var ans = L.GetAnswer();
        }
    }
}
