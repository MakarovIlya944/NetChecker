using System;
using System.Collections.Generic;
using System.IO;
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
        double[] LocalRightPart = new double[4];
        double[,] LocalM = new double[4, 4]
        {
            { 0.01666,0.00833,0.00833,0.00833},
            { 0.00833,0.01666,0.00833,0.00833},
            { 0.00833,0.00833,0.01666,0.00833},
            { 0.00833,0.00833,0.00833,0.01666}
        };
        public SparseMatrix GlobalMatrix;
        public double[] RightPart;
        Func<double, double, double,int, double> FunctionRightPart;
        Func<int, double> Material;

        #region Solver params
        int maxiter = 10000;
        double eps = 1E-14;
        #endregion

        public MatrixGenerator(List<Vector3D> _vector3s, List<Thetra> _thetras, Func<double, double, double, int, double> rightpart, Func<int, double> mat)
        {
            points = _vector3s;
            thetras = _thetras;
            GlobalMatrix = new SparseMatrix(_vector3s.Count, _thetras);
            FunctionRightPart = rightpart;
            Material = mat;
            RightPart = new double[_vector3s.Count];
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
            {
                LocalRightPart[i] = 0;
                for (int j = 0; j < 4; j++)
                {
                    LocalMatrix[i, j] = lambda * Math.Abs(a.Det) / 6 * (a[i, 1] * a[j, 1] + a[i, 2] * a[j, 2] + a[i, 3] * a[j, 3]);
                    LocalRightPart[i] += LocalM[i, j] * FunctionRightPart(points[t[0]].X, points[t[0]].Y, points[t[0]].Z, t.mat);
                }
                LocalRightPart[i] *= Math.Abs(a.Det);
            }
        }

        public void CollectGlobalMatrix()
        {
            foreach (Thetra item in thetras)
            {
                MakeLocalMatrix(item);
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                        GlobalMatrix[item[j], item[k]] += LocalMatrix[j, k];
                    RightPart[item[j]] = LocalRightPart[j];
                }
            }
        }

        public void AccountMainCondition(List<int> b_points, Func<double, double, double, double> bound)
        {
            double cond;
            foreach (int p in b_points)
            {
                cond = bound(points[p].X, points[p].Y, points[p].Z);
                GlobalMatrix[p,p] = 1;
                RightPart[p] = cond;
                

            }
        }

        public void Save()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "kuslau.txt")))
                sw.Write($"{points.Count} {maxiter} {eps}");

            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "x.txt")))
                sw.Write(Enumerable.Range(0, points.Count).Select(x => "1\n").Aggregate((x, y) => x += y));

            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "pr.txt")))
                sw.Write(RightPart.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "di.txt")))
                sw.Write(GlobalMatrix.Diagonal.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));

            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "ig.txt")))
                sw.Write(GlobalMatrix.Ig.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "jg.txt")))
                sw.Write(GlobalMatrix.Jg.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));

            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "ggl.txt")))
                sw.Write(GlobalMatrix.Ggl.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, "ggu.txt")))
                sw.Write(GlobalMatrix.Ggu.Select(x => x.ToString() + "\n").Aggregate((x, y) => x += y));
        }
    }
}
