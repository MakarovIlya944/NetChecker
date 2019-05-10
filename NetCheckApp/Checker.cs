using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCheckApp
{
    public class TooCloseException : ApplicationException
    {
    }

    public class MeshHaventPointException : ApplicationException
    {
    }

    public class NetDontExistException : ApplicationException
    {
    }

    public interface INetChecker
    {
        bool Load(IMesh3D mesh);
        bool Load(List<Vector3D> net, List<Thetra> thetras);
        bool Check();
    }

    public class FEMChecker : Checker, INetChecker
    {
        IMesh3D mesh;
        public bool Check() {

            //HeatProblem problem = new HeatProblem();
            //problem.Initialize(mesh,null,"");
            //problem.Solve();

            //Func<int, double> m = (int mat) => {
            //    return 2;
            //};
            //Func<Vector3D, int, double> f = (Vector3D x, int mat) => {
            //    return 0 * m(mat);
            //};
            //Func<Vector3D, double> bound = (Vector3D x) => {
            //    return x.X + x.Y + x.Z;
            //};
            //Func<Vector3D, double> exact = bound; 

            //List<int> borderPoints = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };

            //MatrixGenerator c = new MatrixGenerator(tree.ToList(), Figures, f, m);
            //double[,] koefs = c.CollectGlobalMatrix();
            //c.AccountMainCondition(borderPoints, bound);
            //c.Save();
            //LOS L = new LOS();
            //L.Load();
            //L.FactorLU();
            //L.Solve();
            //int i = 0;
            //L.GetAnswer().Any(x => Math.Abs(x - exact(tree[i++])) < eps);
            return false;
        }

        public bool Load(IMesh3D mesh) {
            this.mesh = mesh;
            return true;
        }
    }

    public class VolumeChecker : Checker, INetChecker
    {
        //объем отдельных фигур
        public double FiguresValue = 0;
        //объем общий
        public double VolumeValue = 0;

        public VolumeChecker() {
        }

        public bool Check() {
            if(isLoad) {
                MakeThetra();
                return AllValue() == FiguresValue;
            } else
                throw new NetDontExistException();
        }

        private void MakeThetra() {
            double V;
            //-поиск соседей
            for(int i = 0, neib = 0; i < Figures.Count; i++) {
                neib = FindNeiborgs(Figures[i]);
                if(neib == 0)
                    Console.WriteLine($"WARNING! Standalone figure: #{i}");
                if(neib < 4)
                    OutterThetra.Add(i);
                //-проверка объема
                V = Value(i);
#if DEBUG
                Console.WriteLine($"i: {V}");
#endif
                if(V < eps)
                    Console.WriteLine($"WARNING! Value #{i} less then {eps}");
                else
                    FiguresValue += V;
            }
        }

        private double SurfaceIntegral(int numThetra) {
            double result = 0, tmp;
            int[] side;
            for(int i = 0; i < 4; i++)
                if(Figures[numThetra].near[i] == -1) {
                    side = Figures[numThetra].GetSide(i);
                    Vector3D n = Vector3D.Cross(tree[side[1]] - tree[side[0]], tree[side[2]] - tree[side[0]]);
                    Vector3D a = (tree[side[0]] + tree[side[1]] + tree[side[2]]) / 3.0;
                    tmp = n.X * a.X * n.Norm / 2;
                    a = tree[Figures[numThetra].GetOpposite(i)] - a;
                    if(n * a > -eps)
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
            return result;
        }

        //нахождение общего объема
        private double AllValue() {
            double result = 0;
            foreach(int el in OutterThetra)
                result += SurfaceIntegral(el);
            VolumeValue = result;
            return result;
        }

        //нахождение объема одного
        private double Value(int u) {
            int[] i = Figures[u].p;

            return Math.Abs(
                  (tree[i[1]].X - tree[i[0]].X) * (tree[i[2]].Y - tree[i[0]].Y) * (tree[i[3]].Z - tree[i[0]].Z)
                + (tree[i[3]].X - tree[i[0]].X) * (tree[i[1]].Y - tree[i[0]].Y) * (tree[i[2]].Z - tree[i[0]].Z)
                + (tree[i[2]].X - tree[i[0]].X) * (tree[i[3]].Y - tree[i[0]].Y) * (tree[i[1]].Z - tree[i[0]].Z)
                - (tree[i[3]].X - tree[i[0]].X) * (tree[i[2]].Y - tree[i[0]].Y) * (tree[i[1]].Z - tree[i[0]].Z)
                - (tree[i[1]].X - tree[i[0]].X) * (tree[i[3]].Y - tree[i[0]].Y) * (tree[i[2]].Z - tree[i[0]].Z)
                - (tree[i[2]].X - tree[i[0]].X) * (tree[i[1]].Y - tree[i[0]].Y) * (tree[i[3]].Z - tree[i[0]].Z)) / 6.0;
        }

        public bool Load(IMesh3D mesh) {

            tree = new OctoTree(mesh.Center - new Vector3D(1, 1, 1) * mesh.Radius, mesh.Center + new Vector3D(1, 1, 1) * mesh.Radius);
            foreach(Vector3D vector in mesh.Vertices) 
                tree.AddElement(vector);

            Figures = new List<Thetra>();
            foreach(var domain in mesh.AllDomains) {
                if(!domain.IsExtern && !domain.IsBoundary && domain.GeometryElements[0].Geometry.Metadata.Master.ElementType == ElementType.Tetrahedron)
                    foreach(var gemobj in domain.GeometryElements) {
                        Figures.Add(new Thetra(gemobj.Geometry.Indices));
                        Figures[Figures.Count - 1].material = domain.MaterialNumber;
                    }
            }
            isLoad = true;
            return true;
        }
    }

    public class ConnectChecker : Checker, INetChecker
    {

        //для поиска односвязности
        private bool[] VisitedVertecies;
        private bool[] VisitedThetra;

        public ConnectChecker() {
        }

        public bool Check() {
            if(isLoad) {
                VisitedVertecies = new bool[tree.Count];
                VisitedThetra = new bool[Figures.Count];
                MakeThetra();
                return ConnectedСomponent();
            } else
                throw new NetDontExistException();
        }

        private void MakeThetra() {
            //-поиск соседей
            for(int i = 0, neib = 0; i < Figures.Count; i++) {
                neib = FindNeiborgs(Figures[i]);
                if(neib == 0)
                    Console.WriteLine($"WARNING! Standalone figure: #{i}");
                if(neib < 4)
                    OutterThetra.Add(i);
            }
        }

        private bool ConnectedСomponent() {
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

        private bool ThetrasMatch() {
            int t = 0;
            
            //if(VisitedThetra[t]) {OutterThetra.Exists(x => x == t)}
            for(; t < VisitedThetra.Length && (VisitedThetra[t] || !OutterThetra.Contains(t)); t++)
                ;
            //внешние тетраэдры и dfs не соответствуют 
            return t == VisitedThetra.Length;
        }

        private int dfs(int u) {
            int visited = 0;
            foreach(int a in _1(u)) {
                if(!VisitedVertecies[a]) {
                    visited++;
                    VisitedVertecies[a] = true;
                    foreach(int b in _T(a)) {
                        if(!VisitedThetra[b]) {
                            VisitedThetra[b] = true;
                            visited += dfs(b);
                        }
                    }
                }
            }
            return visited;
        }

        //Определяет какие вершины тетраэдра внешние
        private List<int> _1(int numThetra) {
            List<int> ans = new List<int>();
            for(int i = 0, cur = 0; i < 4; i++)
                if(Figures[numThetra].near[i] == -1) {
                    cur = Figures[numThetra].p[i / 3];
                    if(!ans.Contains(cur))
                        ans.Add(cur);

                    cur = Figures[numThetra].p[i / 2 + 1];
                    if(!ans.Contains(cur))
                        ans.Add(cur);

                    cur = Figures[numThetra].p[(i - 3) / 3 + 3];
                    if(!ans.Contains(cur))
                        ans.Add(cur);
                }
            return ans;
        }

        //Определяет внешние тетраэдры в которых есть вершина numVert
        private List<int> _T(int numVert) {
            List<int> ans = new List<int>();
            bool V, _1;
            for(int i = 0, n = Figures.Count; i < n; i++) {
                V = false;
                _1 = false;
                foreach(int a in Figures[i].p)
                    if(numVert == a) {
                        V = true;
                        break;
                    }
                foreach(int b in Figures[i].near)
                    if(-1 == b) {
                        _1 = true;
                        break;
                    }
                if(_1 && V)
                    ans.Add(i);
            }

            return ans;
        }

        public bool Load(IMesh3D mesh) {
            throw new NotImplementedException();
        }
    }

    public class Checker
    {
        public List<Thetra> Figures = new List<Thetra>();
        public Dictionary<int, string> Names = new Dictionary<int, string>();
        protected bool isLoad = false;

        //номера внешних тетраэдров
        protected List<int> OutterThetra = new List<int>();

        //малая величина для SurfaceIntegral
        public double eps = 1E-4;
        //минимальная дистанция в дереве
        public double dist = 1E-1;
        
        //октодерево вершин
        protected OctoTree tree;

        public Checker() {
        }

        public bool Load(List<Vector3D> net, List<Thetra> thetras) {
            try {
                Figures = thetras;
                Vector3D max = net[0], min = net[0];
                net.ForEach(x => { max = Vector3D.Max(max, x); min = Vector3D.Min(min, x); });
                tree = new OctoTree(min, max);
                tree.minDist = dist;
                net.ForEach(x => tree.AddElement(x));
                isLoad = true;
            } catch(Exception) {
                isLoad = false;
                return false;
            }
            return true;
        }
        
        private int PositionNumerSide(IEnumerable<int> a)
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
        protected int FindNeiborgs(Thetra T)
        {
            //поиск соседних тетра
            HashSet<int> finded = new HashSet<int>();
            HashSet<int> looked = new HashSet<int>();
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
    }
}
