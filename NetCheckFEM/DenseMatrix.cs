﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    public class DenseMatrix : IMatrix
    {
        public int dimX = 1, dimY = 1;
        double[,] data;
        public double Det = 0;

        public double this[int x, int y]
        {
            get => data[x, y];
            set => data[x, y] = value;
        }

        public DenseMatrix(int x, int y)
        {
            dimX = x;
            dimY = y;
            data = new double[x, y];
        }

        public static IMatrix DiagonalMatrix(int n, double d)
        {
            var res = new DenseMatrix(n, n);
            for (int i = 0; i < n; i++)
                res[i, i] = d;
            return res;
        }

        public DenseMatrix(int x, int y, double[,] d)
        {
            dimX = x;
            dimY = y;
            data = new double[x, y];
            for(int i=0;i<x;i++)
                for(int j=0;j<y;j++)
                    data[i,j] = d[i, j];
        }

        double GetDet()
        {
            int[][] switches;
            int n, m;
            if (dimX != dimY)
                throw new Exception("Matrix not squad");
            if (dimX == 1)
                return data[0, 0];
            else if (dimX == 2)
            { switches = new int[2][]; n = 2; }
            else if (dimX == 3)
            { switches = new int[6][]; n = 6; }
            else if (dimX == 4)
            { switches = new int[120][]; n = 120; }
            else
                throw new Exception("Not implemented dimension");
            
            double det = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < dimX; j++)
                    det += data[j,switches[i][j]] * switches[i][dimX];

            return det;
        }
        
        double MinorDet(int k, int l)
        {
            if (dimX != dimY)
                throw new Exception("Matrix not squad");
            if(dimX < 1 || dimX > 5)
                throw new Exception("Not implemented dimension");

            double det = 0;
            Enumerable.Range(0,dimX).


            return det;
        }

        IMatrix ReverseDefault()
        {
            if (dimX != dimY)
                throw new Exception("Matrix not squad");
            Det = GetDet();

            DenseMatrix X = new DenseMatrix(dimX, dimX);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    X.data[i, j] = (((i + j) & 1) == 0 ? 1 : -1) * MinorDet(i, j) / Det;
            return X;
        }

        public IMatrix ReverseMatrix()
        {
            if (dimX != dimY)
                throw new Exception("Matrix not squad");
            int n = dimX;
            var a = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    a[i, j] = data[i, j];

            var X = new DenseMatrix(n, n);
            DenseMatrix B = (DenseMatrix)DiagonalMatrix(n, 1);

            for (int k = 0; k < n - 1; k++)
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(a[k, k]) < 1e-14)
                        throw new Exception("Degenerate matrix");
                    double t = a[i, k] / a[k, k];
                    for (int j = 0; j <= k; j++)
                        B[i, j] -= t * B[k, j];
                    for (int j = k+1; j < n; j++)
                    {
                        B[i, j] -= t * B[k, j];
                        a[i, j] -= t * a[k, j];
                    }
                }
            for (int i = 0; i < n; i++)
                X[n - 1, i] = B[n - 1, i] / a[n - 1, n - 1];
            for (int k = n - 2; k >= 0; k--)
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = k + 1; j < n; j++)
                        sum += a[k, j] * X[j, i];
                    X[k, i] = (B[k, i] - sum) / a[k, k];
                }

            X.Det = 1;
            for (int i = 0; i < n; i++)
                X.Det *= a[i, i];
            return X;
        }

        public void Print()
        {
            Console.WriteLine(ToString().Replace("|", Environment.NewLine));
        }

        public static DenseMatrix operator *(DenseMatrix a, DenseMatrix b)
        {
            if (a.dimX != b.dimY || a.dimY != b.dimX)
                throw new Exception("Invalid dimensions");
            var res = new DenseMatrix(a.dimY, a.dimY);
            int n = a.dimY;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < a.dimX; k++)
                        res[i, j] += a[i, k] * b[k, j];
            return res;
        }

        public override string ToString()
        {
            var res = "|";
            for (int i = 0; i < dimY; i++)
            {
                for (int j = 0; j < dimX; j++)
                    res += Math.Round(data[i, j],2) + " ";
                res += "|";
            }
            return res;
        }
    }
}
