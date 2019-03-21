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
            double[] p = new double[4] { 1, 2, 3, 4 };
            Vector a = new Vector(p);
            p[0] = 312;
            p[1] = 312;
        }
    }
}
