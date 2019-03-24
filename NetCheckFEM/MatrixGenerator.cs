using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCheckApp;

namespace NetCheckerFEM
{
    class MatrixGenerator
    {
        List<Vector3D> points;
        List<Thetra> thetras;
        DenseMatrix LocalMatrix = new DenseMatrix(4, 4);
        SparseMatrix GlobalMatrix;

        public MatrixGenerator(List<Vector3D> _vector3s, List<Thetra> _thetras)
        {
            points = _vector3s;
            thetras = _thetras;
        }

        double Material(int mat)
        {
            return 0;
        }
        
        void MakeLocalMatrix(Thetra t)
        {
            double[,] data = new double[4, 4] {
                {1,1,1,1 },
                {points[t[0]].X,points[t[1]].X,points[t[2]].X,points[t[3]].X },
               {points[t[0]].Y,points[t[1]].Y,points[t[2]].Y,points[t[3]].Y },
               {points[t[0]].Z,points[t[1]].Z,points[t[2]].Z,points[t[3]].Z }
            };
            DenseMatrix a = (DenseMatrix)(new DenseMatrix(4, 4, data)).ReverseMatrix();
            double lambda = Material(t.mat);
            
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    LocalMatrix[i, j] = lambda * Math.Abs(a.Det) / 6 * (a[i, 1] * a[j, 1] + a[i, 2] * a[j, 2] + a[i, 3] * a[j, 3]);
        }

        public void CollectGlobalMatrix()
        {
            foreach (Thetra item in thetras)
            {
                MakeLocalMatrix(item);
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        GlobalMatrix[item[j], item[k]] += LocalMatrix[j, k];
            }
        }

        public void Save(string name)
        {

        }
    }
}
