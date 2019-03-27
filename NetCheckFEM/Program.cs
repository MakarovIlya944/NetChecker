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
            
            //NetGenerator a = new NetGenerator();
            //a.Load(@"C:\Work\GMesh\cube.msh");
            double[,] data = new double[3, 3] { { 1, 2, 3}, { 4, 5 ,6 }, { 7, 2, 1 } };
            //double[,] data = new double[2, 2] { { 4, 2 }, { 6, 5 } };
            double[,] data1 = new double[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            double[,] data2 = new double[3, 3] { { -1, -2, -2 }, { 3, 2, 4 }, { -2, -5, -1 } };

            DenseMatrix d1 = new DenseMatrix(3, 3, data1);
            DenseMatrix d2 = new DenseMatrix(3, 3, data2);
            (d1 * d2).Print();

            DenseMatrix a = new DenseMatrix(3, 3, data);
            DenseMatrix b = (DenseMatrix)a.ReverseMatrix();
            b.Print();
            (a * b).Print();
            int aqw = 2;
        }
    }
}
