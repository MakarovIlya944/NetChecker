using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetCheckApp
{
    public class TooCloseException : ApplicationException
    {
    }

    public class MeshHaventPointException : ApplicationException
    {
    }

    public enum CheckerMode { None = 0, ConnectComonent = 1, Value = 2, ConnectComonentValue = 3 };

    public class Checker
    {
        public List<Thetra> Figures = new List<Thetra>();
        public Dictionary<int, string> Names = new Dictionary<int, string>();
        //номера внешних тетраэдров
        public List<int> OutterThetra = new List<int>();
        //объем отдельных фигур
        public double FiguresValue = 0;
        //объем общий
        public double VolumeValue = 0;
        //малая величина
        public double eps = 1E-4;
        //minDist
        public double dist = 1E-1;
        //для поиска односвязности
        private bool[] VisitedVertecies;
        private bool[] VisitedThetra;
        //октодерево вершин
        private OctoTree tree;

        public Checker()
        {

        }

        public bool CheckName(int mat)
        {
            return Names.ContainsKey(mat);
        }

        public void Input(string path)
        {
            //создание массива точек
            string[] str_in = File.ReadAllLines(path);
            int n = Convert.ToInt32(str_in[0]), i = 1;
            VisitedVertecies = new bool[n];
            Vector3D current;
            tree = new OctoTree(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1));
            tree.minDist = dist;
            for (; i <= n; i++)
            {
                Vector3D.TryParse(str_in[i], out current);
                //проверка повторения
                //проверка близости
                tree.AddElement(current);
                if (tree.tooClose)
                {
                    Console.WriteLine($"Точка слишком близко {i}");
                    return;
                }
                //tree.Add(current);
            }

            n = i + Convert.ToInt32(str_in[i]);
            i++;

            //создание массива имен
            for (int j = n + 2, nj = Convert.ToInt32(str_in[j - 1]) + j - 1; j <= nj; j++)
                Names[Convert.ToInt32(str_in[j].Split(' ')[0])] = str_in[j].Split(' ')[1];

            VisitedThetra = new bool[n - i + 1];

            //создание массива Тетра
            for (; i <= n; i++)
                //проверка имен
                if (CheckName(Convert.ToInt32(str_in[i].Split(' ')[4])))
                    Figures.Add(new Thetra(str_in[i].Split(' ')));
        }

        public void Check()
        {
            var mode = CheckerMode.ConnectComonentValue;
            MakeThetra();
            bool isconnect = ConnectedСomponent();
            double allvalue = AllValue();
            Console.WriteLine("Figures add:\t\t\tcorrect");
            if ((mode & CheckerMode.ConnectComonent) == CheckerMode.ConnectComonent)
                Console.WriteLine("Connected component:\t\t\t" + (isconnect ? "correct" : "incorrect"));
            if ((mode & CheckerMode.Value) == CheckerMode.Value)
                Console.WriteLine($"Value: Sum:{FiguresValue} == Numeric:{allvalue}");
        }

        /// <summary>
        /// Проверка близости и повторения вершин
        /// </summary>
        /// <param name="vertices">Список вершин</param>
        /// <returns>True если всё ок</returns>
        public void Check(Vector3D[] vertices)
        {
            int n = vertices.Length;
            VisitedVertecies = new bool[n];
            Vector3D max = vertices.Aggregate((x, y) => new Vector3D(
                Math.Max(Math.Abs(x.X), Math.Abs(y.X)),
                Math.Max(Math.Abs(x.Y), Math.Abs(y.Y)),
                Math.Max(Math.Abs(x.Z), Math.Abs(y.Z))));

            tree = new OctoTree(-max, max);
            tree.minDist = dist;
            for (int i = 0; i < n; i++)
            {
                tree.AddElement(vertices[i]);
                if (tree.tooClose)
                {
                    Console.WriteLine($"ERROR! Point {i} too close!");
                    throw new TooCloseException();
                }
            }
            Console.WriteLine("Vertices distance:\t\t\tcorrect");
        }

        /// <summary>
        /// Проверка единственности компоненты связности и наличия дыр в области
        /// </summary>
        /// <param name="domains"></param>
        /// <returns>True если всё ок</returns>
        /*public void Check(IGeometryDomain[] domains, CheckerMode mode)
        {
            if (mode != CheckerMode.None)
            {
                Console.WriteLine("----------------------------------------NetCheckerApp------------------------------------------------");
                int[] vertecies = new int[5];
                int tmp;
                foreach (var el in domains)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        tmp = tree.FindIndex(el.Mesh[i]);
                        if (tmp != -1)
                            vertecies[i] = tmp;
                        else
                        {
                            Console.WriteLine($"ERROR! Point {el.Mesh[i]} didn't exist at mesh {el.Mesh}!");
                            throw new MeshHaventPointException();
                        }
                    }
                    vertecies[4] = el.MaterialNumber;
                    Figures.Add(new Thetra(vertecies));
                }
                Console.WriteLine("Thetras add:\t\t\tcorrect");

                MakeThetra();
                bool isconnect = ConnectedСomponent();
                double allvalue = AllValue();
                Console.WriteLine("Figures add:\t\t\tcorrect");
                if ((mode & CheckerMode.ConnectComonent) == CheckerMode.ConnectComonent)
                    Console.WriteLine("Connected component:\t\t\t" + (isconnect ? "correct" : "incorrect"));
                if ((mode & CheckerMode.Value) == CheckerMode.Value)
                    Console.WriteLine($"Value: Sum:{FiguresValue} == Numeric:{allvalue}");
            }
        }
        */
        private void MakeThetra()
        {
            double V;
            //-поиск соседей
            for (int i = 0, neib = 0; i < Figures.Count; i++)
            {
                neib = FindNeiborgs(Figures[i]);
                if (neib == 0)
                    Console.WriteLine($"WARNING! Standalone figure: #{i}");
                if (neib < 4)
                    OutterThetra.Add(i);
                //-проверка объема
                V = Value(i);
#if DEBUG
                Console.WriteLine($"i: {V}");
#endif
                if (V < eps)
                    Console.WriteLine($"WARNING! Value #{i} less then {eps}");
                else
                    FiguresValue += V;
            }
        }

        private int PositionNumerSide(List<int> a)
        {
            if (a.Contains(0))
                if (a.Contains(1))
                    if (a.Contains(2))
                        return 0;
                    else
                        return 1;
                else
                    return 2;
            return 3;
        }

        //Возвращает кол-во соседей
        private int FindNeiborgs(Thetra T)
        {
            //поиск соседних тетра
            List<int> finded = new List<int>(3);
            List<int> looked = new List<int>(3);
            int nT = Figures.IndexOf(T), nY;
            int i, j;
            int countSide = 0;
            foreach (Thetra Y in Figures)
                if (T != Y)
                {
                    for (j = 0; j < 4; j++)
                    {
                        for (i = 0; i < 4 && Y.p[i] != T.p[j]; i++) ;
                        if (i != 4)
                        {
                            finded.Add(i);
                            looked.Add(j);
                        }
                    }
                    if (finded.Count == 3)//перезаписывается лишний раз уже известные стороны
                    {
                        nY = Figures.IndexOf(Y);

                        Figures[nT].near[PositionNumerSide(looked)] = nY;
                        Figures[nY].near[PositionNumerSide(finded)] = nT;

                        countSide++;
                    }
                    finded.Clear();
                    looked.Clear();
                }
            return countSide;
        }

        //Определяет какие вершины тетраэдра внешние
        private List<int> _1(int numThetra)
        {
            List<int> ans = new List<int>();
            for (int i = 0, cur = 0; i < 4; i++)
                if (Figures[numThetra].near[i] == -1)
                {
                    cur = Figures[numThetra].p[i / 3];
                    if (!ans.Contains(cur)) ans.Add(cur);

                    cur = Figures[numThetra].p[i / 2 + 1];
                    if (!ans.Contains(cur)) ans.Add(cur);

                    cur = Figures[numThetra].p[(i - 3) / 3 + 3];
                    if (!ans.Contains(cur)) ans.Add(cur);
                }
            return ans;
        }

        //Определяет внешние тетраэдры в которых есть вершина numVert
        private List<int> _T(int numVert)
        {
            List<int> ans = new List<int>();
            bool V, _1;
            for (int i = 0, n = Figures.Count; i < n; i++)
            {
                V = false; _1 = false;
                foreach (int a in Figures[i].p)
                    if (numVert == a)
                    { V = true; break; }
                foreach (int b in Figures[i].near)
                    if (-1 == b)
                    { _1 = true; break; }
                if (_1 && V)
                    ans.Add(i);
            }

            return ans;
        }

        private int dfs(int u)
        {
            int visited = 0;
            foreach (int a in _1(u))
            {
                if (!VisitedVertecies[a])
                {
                    visited++;
                    VisitedVertecies[a] = true;
                    foreach (int b in _T(a))
                    {
                        if (!VisitedThetra[b])
                        {
                            VisitedThetra[b] = true;
                            visited += dfs(b);
                        }
                    }
                }
            }
            return visited;
        }

        private bool ThetrasMatch()
        {
            int t = 0;
            //if(VisitedThetra[t]) {OutterThetra.Exists(x => x == t)}
            for (; t < VisitedThetra.Length && (VisitedThetra[t] || !OutterThetra.Exists(x => x == t)); t++) ;
            //внешние тетраэдры и dfs не соответствуют 
            return t == VisitedThetra.Length;
        }

        private bool ConnectedСomponent()
        {
#if DEBUG
            int a = dfs(0);
            bool b = ThetrasMatch();
            Console.WriteLine($"Tree: {tree.Count}");
            Console.WriteLine($"Dfs: {a}");
            Console.WriteLine($"ThetrasMatch: {b}");
            return tree.Count == a && b;
#else
            return tree.Count == dfs(0) && ThetrasMatch();
#endif
        }

        private double SurfaceIntegral(int numThetra)
        {
            double result = 0, tmp;
            int[] side;
            for (int i = 0; i < 4; i++)
                if (Figures[numThetra].near[i] == -1)
                {
                    side = Figures[numThetra].GetSide(i);
                    Vector3D n = Vector3D.Cross(tree[side[1]] - tree[side[0]], tree[side[2]] - tree[side[0]]);
                    Vector3D a = (tree[side[0]] + tree[side[1]] + tree[side[2]]) / 3.0;
                    tmp = n.X * a.X * n.Norm / 2;
                    a = tree[Figures[numThetra].GetOpposite(i)] - a;
                    if (n * a > -eps)
                        tmp *= -1;
#if DEBUG
                    Console.WriteLine($"\r\nThetra: {numThetra}\r\nNorm: {n}\r\nA: {a}");
                    Console.WriteLine($"Int: {tmp}");
                    Console.WriteLine($"0 :{tree[side[0]]}");
                    Console.WriteLine($"1 :{tree[side[1]]}");
                    Console.WriteLine($"2 :{tree[side[2]]}");
#endif
                    result += tmp;
                }
#if DEBUG
            Console.WriteLine("-------------------------------------------------------------------\r\n");
#endif
            return result;
        }

        //нахождение общего объема
        private double AllValue()
        {
            double result = 0;
            foreach (int el in OutterThetra)
                result += SurfaceIntegral(el);
            VolumeValue = result;
            return result;
        }

        //нахождение объема одного
        private double Value(int u)
        {
            int[] i = Figures[u].p;

            return Math.Abs((tree[i[1]].X - tree[i[0]].X) * (tree[i[2]].Y - tree[i[0]].Y) * (tree[i[3]].Z - tree[i[0]].Z)
                + (tree[i[3]].X - tree[i[0]].X) * (tree[i[1]].Y - tree[i[0]].Y) * (tree[i[2]].Z - tree[i[0]].Z)
                + (tree[i[2]].X - tree[i[0]].X) * (tree[i[3]].Y - tree[i[0]].Y) * (tree[i[1]].Z - tree[i[0]].Z)
                - (tree[i[3]].X - tree[i[0]].X) * (tree[i[2]].Y - tree[i[0]].Y) * (tree[i[1]].Z - tree[i[0]].Z)
                - (tree[i[1]].X - tree[i[0]].X) * (tree[i[3]].Y - tree[i[0]].Y) * (tree[i[2]].Z - tree[i[0]].Z)
                - (tree[i[2]].X - tree[i[0]].X) * (tree[i[1]].Y - tree[i[0]].Y) * (tree[i[3]].Z - tree[i[0]].Z)) / 6.0;
        }
    }
}
