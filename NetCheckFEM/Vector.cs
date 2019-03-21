using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    class Vector
    {
        int dimension = 3;
        double[] data;

        public double X => data[0];
        public double Y => data[1];
        public double Z => data[2];

        public Vector(double[] coord)
        {
            dimension = coord.Length;
            data = new double[dimension];
            data = coord;
        }
    }
}
