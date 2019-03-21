using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    class NetGenerator
    {
        double normar;

        int n, maxiter, iter;
        double eps;

        double[] gglLU, gguLU, diLU;
	    double[] gglA, gguA, diA;
	    double[] diD;
        int[] ig, jg;

	    double[] r, z, x, p, pr, Ax, Ay, x0;
    }
}
