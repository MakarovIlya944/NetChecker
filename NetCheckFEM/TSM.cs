﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetCheckerFEM
{
    //ThreeStepMethod
    class LOS
    {
        double normar;

        int n, maxiter, iter;
        double eps;

        List<double> gglLU, gguLU, diLU;
        List<double> gglA, gguA, diA;
        List<double> diD;
        List<int> ig, jg;

        List<double> r, z, x, p, pr, Ax, Ay, x0;

	    //c=b-a
	    public void sub(List<double> a, List<double> b, List<double> c)
        {
            for (int i = 0; i < n; i++)
                c[i] = b[i] - a[i];
        }

        //return (a,b)
        public double scalar(List<double> a, List<double> b)
        {
            double ans = 0;
            for (int i=0; i < n; i++)
                ans += a[i] * b[i];
            return ans;
        }

        //d=a+b*c
        public void addmult(List<double> a, double b, List<double> c, List<double> d)
        {
            for (int i = 0; i < n; i++)
                d[i] = a[i] + b * c[i];
        }

        public List<double> GetAnswer()
        {
            return x;
        }

        public void Load()
        {
            string line;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            StreamReader file = new StreamReader(Path.Combine(path, "kuslau.txt"));
            line = file.ReadLine();
            n = Int32.Parse(line);
            maxiter = Int32.Parse(line);
            eps = Double.Parse(line);

            diLU = new List<double>(n) { };
            diA = new List<double>(n) { };
            diD = new List<double>(n) { };
            ig = new List<int>(n + 1) { };
            pr = new List<double>(n) { };
            r = new List<double>(n) { };
            z = new List<double>(n) { };
            x = new List<double>(n) { };
            p = new List<double>(n) { };
            Ax = new List<double>(n) { };
            Ay = new List<double>(n) { };
            x0 = new List<double>(n) { };

            using (StreamReader sr = new StreamReader(Path.Combine(path, "x.txt")))
            {
                x0 = sr.ReadToEnd().Split('\n').Select(x => Double.Parse(x)).ToList();
                x = x0.ToList();
            }

            using (StreamReader sr = new StreamReader(Path.Combine(path, "pr.txt")))
                pr = sr.ReadToEnd().Split('\n').Select(x => Double.Parse(x)).ToList();

            using (StreamReader sr = new StreamReader(Path.Combine(path, "di.txt")))
                diA = sr.ReadToEnd().Split('\n').Select(x => Double.Parse(x)).ToList();

            using (StreamReader sr = new StreamReader(Path.Combine(path, "ig.txt")))
                ig = sr.ReadToEnd().Split('\n').Select(x => Int32.Parse(x)).ToList();


            int tmp = ig[n];
            jg = new List<int>(tmp);
            gglA = new List<double>(tmp) { };
            gguA = new List<double>(tmp) { };
            gglLU = new List<double>(tmp) { };
            gguLU = new List<double>(tmp) { };

            using (StreamReader sr = new StreamReader(Path.Combine(path, "jg.txt")))
                jg = sr.ReadToEnd().Split('\n').Select(x => Int32.Parse(x)).ToList();

            using (StreamReader sr = new StreamReader(Path.Combine(path, "ggl.txt")))
                gglA = sr.ReadToEnd().Split('\n').Select(x => Double.Parse(x)).ToList();

            using (StreamReader sr = new StreamReader(Path.Combine(path, "ggu.txt")))
                gguA = sr.ReadToEnd().Split('\n').Select(x => Double.Parse(x)).ToList();
        }


        void Save(string path)
        {
            using (StreamWriter sr = new StreamWriter(path))
                sr.Write(x.Select(a => a.ToString()).Aggregate((x, y) => x + Environment.NewLine + y));
        }

        //Ax = y умножение в разряженном формате
        //y - можно любой
        public void MultMatrix(List<double> x, List<double> y, bool f)
        {
            if (f)
            {
                for (int i = 0; i < n; i++)
                    y[i] = diA[i] * x[i];

                for (int i = 0; i < n; i++)
                    for (int k = ig[i], m = ig[i + 1]; k < m; k++)
                    {
                        y[i] += gglA[k] * x[jg[k]];
                        y[jg[k]] += gguA[k] * x[i];
                    }
            }
            else
                for (int i = 0; i < n; i++)
                    y[i] = diD[i] * x[i];
        }

        public void Solve()
        {
            MultMatrix(x, Ax, true);
            for (int i = 0; i < n; i++)
            {
                r[i] = pr[i] - Ax[i];
                z[i] = r[i];
            }
            MultMatrix(z, Ax, true);
            List<double> tttt = new List<double>();
            tttt = Ax;
            Ax = p;
            p = tttt;
            normar = scalar(r, r);
            for (iter = 0; iter < maxiter && Math.Abs(normar) > eps && Math.Abs(normar) > 1E-30; iter++)
            {
                double normap = scalar(p, p);
                double a = scalar(p, r) / normap;
                addmult(x, a, z, x);
                addmult(r, -a, p, r);
                MultMatrix(r, Ax, true);
                double b = -scalar(p, Ax) / normap;
                addmult(r, b, z, z);
                normar -= a * a * normap;
                addmult(Ax, b, p, p);
            }
        }

        public void FactorD()
        {
            try
            {
                for (int i = 0; i < n; i++)
                {
                    if (diA[i] < 1E-30)
                        throw new Exception();
                    diD[i] = 1 / Math.Sqrt(diA[i]);
                }
            }

            catch (Exception)
            {
                Console.WriteLine("Divided by zero!");
            }
        }

        public void SolveD()
        {
            MultMatrix(x, Ax, true);
            for (int i = 0; i < n; i++)
            {
                r[i] = diD[i] * (pr[i] - Ax[i]);
                z[i] = diD[i] * r[i];
            }
            MultMatrix(z, Ax, true);
            MultMatrix(Ax, p, false);
            normar = scalar(r, r);
            for (iter = 0; iter < maxiter && Math.Abs(normar) > eps && Math.Abs(normar) > 1E-30; iter++)
            {
                double normap = scalar(p, p);
                double a = scalar(p, r) / normap;
                addmult(x, a, z, x);
                addmult(r, -a, p, r);

                MultMatrix(r, Ax, false);
                MultMatrix(Ax, Ay, true);
                MultMatrix(Ay, Ay, false);
                double b = -scalar(p, Ay) / normap;

                addmult(Ax, b, z, z);
                normar -= a * a * normap;
                addmult(Ay, b, p, p);

            }

        }

        public void FactorLU()
        {
            int j0 = 0;
            for (int i = 0; i < n; i++)
            {
                int m = ig[i + 1] - ig[i];
                double sum = 0;
                if (m != 0)
                {
                    int jm = j0 + m;
                    for (int j = j0; j < jm; j++)
                    {
                        int stolbez = jg[j];
                        double sumL = 0, sumU = 0;
                        if (j != j0)//npred!=0
                            for (int l = ig[i], nl = j, nu = ig[stolbez + 1]; l < nl; l++)
                                for (int u = ig[stolbez]; u < nu; u++)
                                {
                                    int gl = jg[l], gu = jg[u];
                                    if (gl == gu)
                                    {
                                        sumL += gguLU[u] * gglLU[l];
                                        sumU += gguLU[l] * gglLU[u];
                                    }
                                }
                        gglLU[j] = (gglA[j] - sumL) / diLU[jg[j]];
                        gguLU[j] = (gguA[j] - sumU) / diLU[jg[j]];
                        sum += gglLU[j] * gguLU[j];
                    }
                    j0 = jm;
                }
                diLU[i] = Math.Sqrt(diA[i] - sum);
            }
        }

        //Uy = x решение слау
        //н - любой
        public void SLAEU(List<double> x, List<double> y)
        {
            for (int i = 0; i < n; i++) y[i] = x[i];
            for (int i = n - 1; i >= 0; i--)
            {
                y[i] /= diLU[i];
                int kend = ig[i + 1];
                int k = ig[i];
                for (; k != kend; k++)
                    y[jg[k]] -= y[i] * gguLU[k];
            }
        }

        //Ly = x решение слау
        //y - любой
        public void SLAEL(List<double> x, List<double> y)
        {
            double sum;
            for (int i = 0; i < n; i++)
            {
                sum = 0;
                int k0 = ig[i];
                int kend = ig[i + 1];
                for (int k = k0; k < kend; k++)
                    sum += y[jg[k]] * gglLU[k];
                y[i] = (x[i] - sum) / diLU[i];
            }
        }

        public void InitLU()
        {
            MultMatrix(x, r, true);
            sub(r, pr, r);
            SLAEL(r, r);

            for (int i = 0; i < n; i++)
                z[i] = r[i];

            SLAEU(z, z);

            MultMatrix(z, p, true);
            SLAEL(p, p);

            normar = scalar(r, r);
        }

        public void SolveLU()
        {
            InitLU();
            for (iter = 0; iter < maxiter && normar > eps && normar > 1E-30; iter++)
            {
                double normap = scalar(p, p);
                double a = scalar(p, r) / normap;
                addmult(x, a, z, x);
                addmult(r, -a, p, r);

                //(L-1)A(U-1)r = Ay
                //U-1r = Ax
                SLAEU(r, Ax);
                MultMatrix(Ax, Ay, true);
                SLAEL(Ay, Ay);
                double b = -scalar(p, Ay) / normap;

                addmult(Ax, b, z, z);
                normar = Math.Abs(normar - a * a * normap);
                addmult(Ay, b, p, p);

                if (Math.Abs(a * a * normap) < 1E-20)
                    InitLU();
            }
        }

        public void ClearX()
        {
            for (int i = 0; i < n; i++)
                x[i] = x0[i];
        }
    };
}
