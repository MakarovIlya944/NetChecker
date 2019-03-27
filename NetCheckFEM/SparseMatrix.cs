using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCheckApp;

namespace NetCheckerFEM
{
    class SparseMatrix 
    {
        public int dim = 1;
        double[][] data;//2 x n
        public double Det = 0;

        double[] di;
        int[] ig, jg;
        int curElem = 0;

        public double[] Diagonal { get => di; }
        public int[] Ig { get => ig; }
        public int[] Jg { get => jg; }
        public double[] Ggu { get => data[1]; }
        public double[] Ggl { get => data[0]; }
        
        public double this[int x, int y]
        {
            get {
                if (x > dim || y > dim)
                    throw new Exception("Not found");
                if (x == y)
                    return di[x];

                int i = ig[x];
                for (int n = ig[x + 1] + 1; i < n && jg[i - ig[x]] != y; i++) ;
                if (i == ig[x + 1] - ig[x] + 1)
                    throw new Exception("Not found");

                if (x < y)
                    return data[1][i];
                else
                    return data[0][i];
            }
            set
            {
                if (x > dim || y > dim)
                    throw new Exception("Not found");
                if (x == y)
                    di[x] = value;

                int i = ig[x];
                for (int n = ig[x + 1] + 1; i < n && jg[i - ig[x]] != y; i++) ;
                if (i == ig[x + 1] - ig[x] + 1)
                    throw new Exception("Not found");

                if (x < y)
                    data[1][i] = value;
                else
                    data[0][i] = value;
            }
        }

        public SparseMatrix(int n)
        {
            dim = n;
            data = new double[2][];
            data[0] = new double[n];
            data[1] = new double[n];
            di = new double[n];
        }

        public SparseMatrix(int num, List<Thetra> thetras)
        {
            HashSet<int>[] edges = new HashSet<int>[num];
            for(int i = 0;i<num;i++)
                edges[i] = new HashSet<int>();
            foreach (Thetra item in thetras)
                for (int i = 0; i < 4; i++)
                    for (int j = i + 1; j < 4; j++)
                        edges[item[j]].Add(item[i]);
            int nJg = 0;
            ig = new int[num + 1];
            ig[0] = 0;
            for (int i = 1; i < num; i++)
            {
                ig[i] = ig[i - 1] + edges[i].Count;
                nJg += edges[i].Count;
            }
            jg = new int[nJg];
            for (int i=0, k=0; i < num; i++)
                foreach(int var in edges[i])
                    jg[k++] = var;
            data = new double[2][];
            data[0] = new double[nJg];
            data[1] = new double[nJg];
        }

        public void Add(bool isUpperTriangle, double d)
        {
            if (curElem > dim)
            { Console.WriteLine("WARNING! Current value overflow"); curElem = 0; }
            data[isUpperTriangle ? 1 : 0][curElem++] = d;
        }
    }
}
