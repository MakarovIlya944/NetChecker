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
        double[,] data;
        public double Det = 0;

        double[] di;
        int[] ig, jg;
        int curElem = 0;

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

        public SparseMatrix(int num, List<Thetra> thetras)
        {
            HashSet<int>[] edges = new HashSet<int>[num];
            foreach (Thetra item in thetras)
                for (int i = 0; i < 4; i++)
                    for (int j = i + 1; j < 4; j++)
                        edges[item[j]].Add(item[i]);
            int nJg = 0;
            ig[0] = 0;
            for (int i = 0; i < num; i++)
            {
                ig[i] = ig[i - 1] + edges[i].Count;
                nJg += ig[i];
            }
            jg = new int[nJg];
            for (int i=0, k=0; i < num; i++)
                foreach(int var in edges[i])
                    jg[k++] = var;
            data = new double[nJg, nJg];
        }
    }
}
