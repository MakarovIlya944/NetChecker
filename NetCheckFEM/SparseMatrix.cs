using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    class SparseMatrix 
    {
        public int dim = 1;
        double[,] data;
        public double Det = 0;

        double[] di;
        int[] ig, jg;

        public double this[int x, int y]
        {
            get {
                if (x < y)
                    return data[x, jg[ig[x]]];
                else if (x == y)
                    return di[x];
                else
                    return data[jg[ig[x]], x];
            }
            set
            {
                if (x < y)
                    data[x, jg[ig[x]]] = value;
                else if (x == y)
                    di[x] = value;
                else
                    data[jg[ig[x]], x] = value;
            }
        }

        public SparseMatrix(int n)
        {
            dim = n;
            data = new double[n, n];
            di = new double[n];
        }
    }
}
