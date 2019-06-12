using NetCheckApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace NetCheckApp {
    public class TooCloseException : ApplicationException {
    }

    public class MeshHaventPointException : ApplicationException {
    }

    public class NetDontExistException : ApplicationException {
        public NetDontExistException(string s) : base(s) { }
    }

    public interface INetChecker {
        bool Load(IMesh3D mesh);
        bool Load(IEnumerable<Vector3D> net, IEnumerable<Thetra> thetras);
        bool Check();
    }

    //public class FEMChecker : Checker, INetChecker {

    //    IMesh3D mesh;
    //    TaskProperties task;
    //    Func<Vector3D, double> u = p => p.X + 2 * p.Y - 2;
    //    Func<Vector3D, Vector3D> gradU = p => new Vector3D(1, 2, 0);
    //    Func<Vector3D, double> f = p => 0;
    //    internal class TestPropertyCollection : IMaterialPropertyCollection {
    //        public Dictionary<string, IMaterialProperty> Properties { get; } = new Dictionary<string, IMaterialProperty>();
    //        public bool TryGetProperty(string name, out IMaterialProperty property) => Properties.TryGetValue(name, out property);
    //    }
    //    internal class TestMaterialProperty : IMaterialProperty {
    //        public string Name { get; set; }
    //        public ICalculator Calculator { get; set; }
    //    }

    //    public bool Check() {
    //        PrepareHeatProblem();
    //        //var mesh = CreateMesh(meshparameter, task.MatCat);
    //        var PsiBasisType = BasisFunctionTypes.Lagrange;
    //        var PsiBasisOrder = 1;

    //        foreach(var elem in mesh.Domains.SelectMany(d => d.GeometryElements).Cast<IScalarFemElement>())
    //            elem.SetPsiFunctions(PsiBasisType, PsiBasisOrder);
    //        foreach(var elem in mesh.Domains.SelectMany(d => d.Boundary().GeometryElements).Cast<IScalarFemElement>())
    //            elem.SetPsiFunctions(PsiBasisType, PsiBasisOrder);

    //        var problem = new HeatProblem();

    //        var path = Guid.NewGuid().ToString();
    //        Directory.CreateDirectory(path);
    //        problem.Initialize(mesh, task, path);

    //        problem.Solve();
    //        var (l2Error, energyError) = problem.CalculateError(u, gradU);
    //        return l2Error < eps;
    //    }

    //    public bool Load(IMesh3D mesh) {
    //        this.mesh = mesh;
    //        isLoad = true;
    //        return true;
    //    }

    //    private void PrepareHeatProblem() {
    //        task = new TaskProperties(true);
    //        var m1 = new CatalogManager.SimpleMaterial() {
    //            CatNum = 0,
    //            Color = Color.BLUE,
    //            IsReserved = false,
    //            MaterialType = MaterialTypes.Scalar,
    //            Name = "material0",
    //        };
    //        var coefs = new TestPropertyCollection();
    //        m1.Coefs = coefs;
    //        coefs.Properties.Add("Lambda", new TestMaterialProperty() { Name = "Lambda", Calculator = Calculator.CreateConst("lambda", 2.0) });
    //        coefs.Properties.Add("RoCp", new TestMaterialProperty() { Name = "RoCp", Calculator = Calculator.CreateConst("RoCp", 0.0) });
    //        coefs.Properties.Add("Velocity", new TestMaterialProperty() { Name = "Velocity", Calculator = Calculator.CreateConst("Velocity", new Vector3D(0, 0, 0)) });
    //        coefs.Properties.Add("F", new TestMaterialProperty() { Name = "F", Calculator = Calculator.CreateCoordDep("f", p => 2 * f(p)) });
    //        task.MatCat.Add(m1);
    //        var bm = new CatalogManager.SimpleBoundary() {
    //            CatNum = 10,
    //            Color = Color.BLUE,
    //            IsReserved = false,
    //            ConditionType = BoundaryTypes.DirichletBoundary,
    //            Name = "U"
    //        };
    //        task.BoundCat.Add(bm);
    //        coefs = new TestPropertyCollection();
    //        bm.Coefs = coefs;
    //        coefs.Properties.Add("Ug", new TestMaterialProperty() { Name = "Ug", Calculator = Calculator.CreateCoordDep("U", u) });

    //        bm = new CatalogManager.SimpleBoundary() {
    //            CatNum = 11,
    //            Color = Color.BLUE,
    //            IsReserved = false,
    //            ConditionType = BoundaryTypes.NeumannBoundary,
    //            Name = "Theta"
    //        };
    //        task.BoundCat.Add(bm);
    //        coefs = new TestPropertyCollection();
    //        bm.Coefs = coefs;
    //        coefs.Properties.Add("Theta", new TestMaterialProperty() { Name = "Theta", Calculator = Calculator.CreateCoordDep("dU/dn", p => -2 * gradU(p) * Vector3D.XAxis) });
    //    }
    //}

    public class VolumeChecker : Checker, INetChecker {
        //объем отдельных фигур
        public double FiguresValue = 0;
        //объем общий
        public double VolumeValue = 0;

        public VolumeChecker() {
        }

        public bool Check() {
            if(isLoad) {
                MakeThetra();
                AllValue();
#if DEBUG
                Console.WriteLine($"All: {VolumeValue} FiguresValue: {FiguresValue}");
                return Math.Abs(VolumeValue - FiguresValue) < eps;
#endif
                return Math.Abs(VolumeValue - FiguresValue) < eps;
            } else
                throw new NetDontExistException("VolumeChecker Error: Сетка не задана");

        }

        private void MakeThetra() {
            double V;
            //-поиск соседей
            for(int i = 0, neib = 0; i < Figures.Count; i++) {
                neib = FindNeiborgs(Figures[i]);
                if(neib == 0) {
                    Console.WriteLine($"WARNING! Standalone figure: #{i}");
                }
                if(neib < 4)
                    OutterThetra.Add(i);
                //-проверка объема
                V = Value(i);
#if DEBUG
                Console.WriteLine($"i: {V}");
#endif
                if(V < eps) {
                    Console.WriteLine($"WARNING! Value #{i} less then {eps}");
                } else
                    FiguresValue += V;
            }
        }

        private double SurfaceIntegral(int numThetra) {
            double result = 0, integral;
            int[] side;
            for(int i = 0; i < 4; i++)
                if(Figures[numThetra].near[i] == -1) {
                    side = Figures[numThetra].GetSide(i);
                    Vector3D n = Vector3D.Cross(tree[side[1]] - tree[side[0]], tree[side[2]] - tree[side[0]]);
                    Vector3D a = (tree[side[0]] + tree[side[1]] + tree[side[2]]) / 3.0;
                    integral = n.X * a.X / 2;
                    a = tree[Figures[numThetra].GetOpposite(i)] - a;
                    if(n * a > -eps)
                        integral *= -1;
#if DEBUG
                    Console.WriteLine($"\r\nThetra: {numThetra}\r\nNorm: {n}\r\nA: {a}");
                    Console.WriteLine($"Int: {integral}");
                    Console.WriteLine($"0 :{tree[side[0]]}");
                    Console.WriteLine($"1 :{tree[side[1]]}");
                    Console.WriteLine($"2 :{tree[side[2]]}");

#endif
                    result += integral;
                }
            return result;
        }

        //нахождение общего объема
        private void AllValue() {
            VolumeValue = 0;
            foreach(int el in OutterThetra)
                VolumeValue += SurfaceIntegral(el);
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

            foreach(var domain in mesh.AllDomains) {
                if(domain.GeometryElements.All(x => x.Geometry.Metadata.Master.ElementType == ElementType.Tetrahedron))
                    foreach(var gemobj in domain.GeometryElements) {
                        Figures.Add(new Thetra(gemobj.Geometry.Indices));
                        Figures[Figures.Count - 1].material = domain.MaterialNumber;
                    }
            }

            isLoad = true;

            return true;
        }
    }

    public class ConnectChecker : Checker, INetChecker {


        public ConnectChecker() {
        }

        public bool Check() {
            if(isLoad) {
                VisitedThetra = new bool[Figures.Count];
                VisitedVertecies = new bool[tree.Count];
                Point2Thetra = new List<int>[tree.Count];
                for(int i = 0; i < Point2Thetra.Length; i++)
                    Point2Thetra[i] = new List<int>();
                MakeThetra();
                return ConnectedСomponent();
            } else
                throw new NetDontExistException("ConnectChecker Error: Сетка не задана");
        }

        private void MakeThetra() {
            //-поиск соседей
            for(int i = 0, neib = 0; i < Figures.Count; i++) {
                neib = FindNeiborgs(Figures[i]);
                foreach(int j in Figures[i].p)
                    Point2Thetra[j].Add(i);
                if(neib == 0) {
                    Console.WriteLine($"WARNING! Standalone figure: #{i}");
                }
                if(neib < 4)
                    OutterThetra.Add(i);
            }
        }

        private int FindFirstOutter() {
            //int i = 0;
            //for(; i < Figures.Count && Array.Find(Figures[i].near, x => x == -1) != -1; i++) ;
            return OutterThetra[0];
        }

        private int CountOutterVertices() {
            HashSet<int> o = new HashSet<int>();
            for(int i = 0; i < Figures.Count; i++)
                o.UnionWith(_1(i));
            return o.Count;
        }

        private bool ConnectedСomponent() {
#if DEBUG
            int a = dfs(FindFirstOutter());
            bool b = ThetrasMatch();
            Console.WriteLine($"Tree: {tree.Count}");
            Console.WriteLine($"Dfs: {a}");
            Console.WriteLine($"ThetrasMatch: {b}");
            return CountOutterVertices() == a && b;
#else
            return CountOutterVertices() == dfs(FindFirstOutter()) && ThetrasMatch();
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


        public bool Load(IMesh3D mesh) {

            tree = new OctoTree(mesh.Center - new Vector3D(1, 1, 1) * mesh.Radius, mesh.Center + new Vector3D(1, 1, 1) * mesh.Radius);
            foreach(Vector3D vector in mesh.Vertices)
                tree.AddElement(vector);

            foreach(var domain in mesh.AllDomains) {
                if(domain.GeometryElements.All(x => x.Geometry.Metadata.Master.ElementType == ElementType.Tetrahedron))
                    foreach(var gemobj in domain.GeometryElements) {
                        Figures.Add(new Thetra(gemobj.Geometry.Indices));
                        Figures[Figures.Count - 1].material = domain.MaterialNumber;
                    }
            }


            isLoad = true;

            return true;
        }
    }

    public class Checker {
        public List<Thetra> Figures = new List<Thetra>();
        public Dictionary<int, string> Names = new Dictionary<int, string>();
        protected bool isLoad = false;

        //номера внешних тетраэдров
        protected List<int> OutterThetra = new List<int>();

        //малая величина для SurfaceIntegral
        public double eps = 1E-12;
        //минимальная дистанция в дереве
        public double dist = 1E-1;

        //октодерево вершин
        protected OctoTree tree;

        //для поиска односвязности
        protected bool[] VisitedVertecies;
        protected bool[] VisitedThetra;
        protected List<int>[] Point2Thetra;

        public Checker() {
        }

        public bool Load(IEnumerable<Vector3D> net , IEnumerable<Thetra> thetras) {
            try {
                Figures = thetras.ToList();
                Vector3D max = net.ElementAt(0), min = max;
                foreach(var x in net) {
                    max = Vector3D.Max(max, x); min = Vector3D.Min(min, x);
                }
                tree = new OctoTree(min, max);
                tree.minDist = dist;
                foreach(var x in net) tree.AddElement(x);
                isLoad = true;
            } catch(Exception) {
                isLoad = false;
                return false;
            }
            return true;
        }

        public void PrintLog(string filename) {
            if(!isLoad)
                return;
            using(var writer = new StreamWriter(filename)) {
                writer.WriteLine("a = 1;");
                int i = 0;
                foreach(var el in tree)
                    writer.WriteLine($"Point({i++}) = {{{el.X}, {el.Y}, {el.Z}, a}};");
                i = 0;
                foreach(var el in Figures)
                    for(int j = 0; j < 4; j++)
                        for(int k = 1; k < 4; k++)
                            writer.WriteLine($"Line({i++}) = {{{el[j]}, {el[k]}}};");
            }
        }

        private int PositionNumerSide(IEnumerable<int> a) {
            if(a.Contains(0))
                if(a.Contains(1))
                    if(a.Contains(2))
                        return 0;
                    else
                        return 1;
                else
                    return 2;
            return 3;
        }

        //Возвращает кол-во соседей
        protected int FindNeiborgs(Thetra T) {
            //поиск соседних тетра
            HashSet<int> finded = new HashSet<int>();
            HashSet<int> looked = new HashSet<int>();
            int nT = Figures.IndexOf(T), nY;
            int i, j;
            int countSide = 0;
            foreach(Thetra Y in Figures)
                if(T != Y) {
                    for(j = 0; j < 4; j++) {
                        for(i = 0; i < 4 && Y.p[i] != T.p[j]; i++) ;
                        if(i != 4) {
                            finded.Add(i);
                            looked.Add(j);
                        }
                    }
                    if(finded.Count == 3)//перезаписывается лишний раз уже известные стороны
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

        protected int dfs(int u) {
            int visited = 0;
            foreach(int a in _1(u)) {
                if(!VisitedVertecies[a]) {
                    visited++;
                    VisitedVertecies[a] = true;
                    foreach(int b in _T(a)) {
                        if(b != -1 && !VisitedThetra[b]) {
                            VisitedThetra[b] = true;
                            visited += dfs(b);
                        }
                    }
                }
            }
            return visited;
        }

        /// <summary>
        /// Определяет какие вершины тетраэдра внешние
        /// </summary>
        /// <param name="numThetra">Индекс тетраэдра в Figures</param>
        /// <returns>Все внешние вершины тетраэдра</returns>
        protected IEnumerable<int> _1(int numThetra) {
            HashSet<int> ans = new HashSet<int>();
            for(int i = 0; i < 4; i++)
                if(Figures[numThetra].near[i] == -1) {
                    ans.Add(Figures[numThetra].p[i / 3]);
                    ans.Add(Figures[numThetra].p[i / 2 + 1]);
                    ans.Add(Figures[numThetra].p[(i - 3) / 3 + 3]);
                }
            return ans;
        }

        //Определяет внешние тетраэдры в которых есть вершина numVert 
        protected List<int> _T(int numVert) {
            List<int> ans = new List<int>();
            return Point2Thetra[numVert].Where(x => Figures[x].near.Any(y => y == -1)).ToList();
        }
    }
}
