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

                int isUp = 0;
                if(x < y)
                {
                    isUp = x;
                    x = y;
                    y = isUp;
                    isUp = 1;
                }
                int i = ig[x];
                for (int n = ig[x + 1] + 1; i < n && jg[i] != y; i++) ;
                if (i == ig[x + 1] + 1)
                    throw new Exception("Not found");

                return data[isUp][i];
            }
            set
            {
                if (x > dim || y > dim)
                    throw new Exception("Not found");
                if (x == y)
                    di[x] = value;
                else
                {
                    int isUp = 0;
                    if (x < y)
                    {
                        isUp = x;
                        x = y;
                        y = isUp;
                        isUp = 1;
                    }
                    int i = ig[x];
                    for (int n = ig[x + 1] + 1; i < n && jg[i] != y; i++) ;
                    if (i == ig[x + 1] + 1)
                        throw new Exception("Not found");

                    data[isUp][i] = value;
                }
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
            dim = num;
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
            ig[1] = 0;
            for (int i = 2; i <= num; i++)
            {
                ig[i] = ig[i - 1] + edges[i-1].Count;
                nJg += edges[i-1].Count;
            }
            jg = new int[nJg];
            for (int i = 0, k = 0; i < num; i++)
                foreach (int var in edges[i].OrderBy(x => x))
                    jg[k++] = var;
            data = new double[2][];
            data[0] = new double[nJg];
            data[1] = new double[nJg];
            di = new double[num];
        }

        public void ZeroRow(int p)
        {
            int ibeg, iend;
            bool flag = true;
            ibeg = ig[p];
            iend = ig[p+1];
            for (int i= ibeg, n = iend; i < n; i++)
                data[0][i] = 0;

            for (int i = p + 1, n = dim, tmp = -1, ind; i < n; i++)
            {
                iend = ig[i+1];
                ibeg = ig[i];
                if ((iend > 0 && jg[iend - 1] < p) || jg[ibeg] > p)
                    continue;
                while (jg[ibeg] != p && flag)//binary find
                {
                    ind = ((ibeg + iend) % 2) != 0 ? (ibeg + iend) / 2 + 1 : (ibeg + iend) / 2;
                    flag = ind != tmp;
                    if (jg[ind] <= p)
                        ibeg = ind;
                    else
                        iend = ind;
                    tmp = ind;
                }
                if (flag)//если нашли индекс в ibeg
                    data[1][ibeg] = 0;
                else
                    flag = true;
            }
        }

        public void Print()
        {
            int ind = 0;
            for (int i = 0; i < dim; i++)
            {
                Console.Write($"#{i:D2}\t");
                for (int j = 0; j < i; j++)
                    if (jg[ind] == j)
                    { Console.Write($"{Math.Round(data[0][ind],2)} "); ind++; }
                    else Console.Write("0.00 ");
                Console.Write($"{Math.Round(di[i], 2)} ");
                for (int j = i + 1; j < dim; j++)
                {
                    int k = ig[j], n = ig[j + 1];
                    for (; k < n; k++)
                        if (jg[k] == i)
                        { Console.Write($"{Math.Round(data[1][k], 2)} "); break; }
                    if(k==n)
                        Console.Write("0.00 ");
                }
                Console.WriteLine();
            }
        }

        public void Add(bool isUpperTriangle, double d)
        {
            if (curElem > dim)
            { Console.WriteLine("WARNING! Current value overflow"); curElem = 0; }
            data[isUpperTriangle ? 1 : 0][curElem++] = d;
        }
    }
}
