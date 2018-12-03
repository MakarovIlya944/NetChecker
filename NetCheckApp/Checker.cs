using System;
using System.Collections.Generic;
using TelmaQuasar;
using System.IO;

namespace NetCheckApp
{
	public class Checker
	{
		public List<Vector3D> Points = new List<Vector3D>();
		public List<Thetra> Figures = new List<Thetra>();
		public Dictionary<int, string> Names = new Dictionary<int, string>();
		//номера внешних тетраэдров
		public List<int> OutterThetra = new List<int>();
		public double eps = 1E-10;
        //шаг интегрирования
        private double dz = 1E-4;
        //объем отдельных фигур
		public double FiguresValue = 0;
        //объем общий
        public double VolumeValue = 0;
        //для поиска односвязности
		private bool[] VisitedVertecies;
		private bool[] VisitedThetra;
        //октодерево вершин
        private OctoTree tree;
        //список тетраэдров для каждой вершины для поиска площади
        private HashSet<int>[] point2Thetra;

        public Checker()
        {
            
        }

		//Проверять через октодерева
		public bool CheckOneness(Vector3D v)
		{
			foreach (Vector3D u in Points)
				if (u == v)
					return false;
			return true;
		}

		public bool CheckNeighbourhood(Vector3D v)
		{
			foreach (Vector3D u in Points)
				if (Vector3D.Distance(u, v) < eps)
					return false;
			return true;
		}

		public bool CheckName(int mat)
		{
			return Names.ContainsKey(mat);
		}

		public void Input()
		{
			
			//создание массива точек
			string[] str_in = File.ReadAllLines("in.txt");
			int n = Convert.ToInt32(str_in[0]), i = 1;
			VisitedVertecies = new bool[n];
			Vector3D current;
            tree = new OctoTree(new Vector3D(0,0,0), new Vector3D(1,1,1));
            tree.minDist = 0.1;
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
                Points.Add(current);
			}

			n = i + Convert.ToInt32(str_in[i]);
			i++;

			//создание массива имен
			for (int j = n + 2, nj = Convert.ToInt32(str_in[j - 1]) + j - 1; j <= nj; j++)
				Names[Convert.ToInt32(str_in[j].Split(' ')[0])] = str_in[j].Split(' ')[1];

			VisitedThetra = new bool[n - i + 1];
            point2Thetra = new HashSet<int>[n];

            //создание массива Тетра
            for (; i <= n; i++)
			{
                //проверка имен
                if (CheckName(Convert.ToInt32(str_in[i].Split(' ')[4])))
                {
                    Figures.Add(new Thetra(str_in[i].Split(' ')));
                    //заполнение связи от точки к тетраэдрам
                    foreach (int p in Figures[Figures.Count - 1].p)
                    {
                        if (point2Thetra[p] == null)
                            point2Thetra[p] = new HashSet<int>();
                        point2Thetra[p].Add(Figures.Count - 1);
                    }
                }
			}
		}

		public void MakeThetra()
		{
			double V;
			//-поиск соседей
			for (int i = 0, neib = 0; i < Figures.Count; i++)
			{
				neib = FindNeiborgs(Figures[i]);
				if (neib == 0)
					Console.WriteLine("Отдельная фигура");
				if (neib < 4)
					OutterThetra.Add(i);
				//-проверка объема
				V = Value(i);
				if (V < 1E-8)
					Console.WriteLine($"Нулевой объём у тэтрадра #{i}");
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
		public int FindNeiborgs(Thetra T)
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

		public bool ConnectedСomponent()
		{
			return OutterThetra.Count == dfs(0);
		}

        private double CrossArea(int ThetraId, double z)
        {
            double t;
            Vector3D tmp;
            List<Vector3D> planePoints = new List<Vector3D>(4);
            foreach (int a in Figures[ThetraId].p)
                foreach (int b in Figures[ThetraId].p)
                    if (a != b && Math.Sign(tree[a].Z - z) * Math.Sign(tree[b].Z - z) <= 0)
                    {
                        if (Math.Abs(tree[b].Z - tree[a].Z) < eps)
                        {
                            if (Math.Abs(tree[b].Z - z) < eps)
                            {
                                if (!planePoints.Contains(tree[b]))
                                    planePoints.Add(tree[b]);
                                if (!planePoints.Contains(tree[a]))
                                    planePoints.Add(tree[a]);
                            }
                        }
                        else
                        {
                            //проверка близости векторов
                            t = (z - tree[a].Z) / (tree[b].Z - tree[a].Z);
                            tmp = tree[a] + t * (tree[b] - tree[a]);
                            if(!planePoints.Contains(tmp))
                                planePoints.Add(tmp);
                        }
                    }

            if (planePoints.Count == 3)
                return Math.Abs(Vector3D.Cross((planePoints[1] - planePoints[0]), (planePoints[2] - planePoints[0])).Norm/2);
            return 0;
        }

        private double CrossArea(double z)
        {
            double result = 0;
            foreach(int el in GetThetras(tree.Find(z)))
                result += CrossArea(el, z);
            return result;
        }

		//нахождение общего объема
		public double AllValue()
		{
            //по всем слоям(dz)
            //выбрать слой(zc,octotree)
            //посчитать площадь
            //v+=si*dz
            double result = 0;
            int layersCapacity = (int)((tree.GetMaxZ() - tree.GetMinZ()) / dz) + 1;
            for (int k = 0; k < layersCapacity; k++)
               result += CrossArea(k * dz) * dz;

            return result;
		}

		//нахождение объема одного
		public double Value(int u)
		{
			int[] i = Figures[u].p;

			return Math.Abs((Points[i[1]].X - Points[i[0]].X) * (Points[i[2]].Y - Points[i[0]].Y) * (Points[i[3]].Z - Points[i[0]].Z)
				+ (Points[i[3]].X - Points[i[0]].X) * (Points[i[1]].Y - Points[i[0]].Y) * (Points[i[2]].Z - Points[i[0]].Z)
				+ (Points[i[2]].X - Points[i[0]].X) * (Points[i[3]].Y - Points[i[0]].Y) * (Points[i[1]].Z - Points[i[0]].Z)
				- (Points[i[3]].X - Points[i[0]].X) * (Points[i[2]].Y - Points[i[0]].Y) * (Points[i[1]].Z - Points[i[0]].Z)
				- (Points[i[1]].X - Points[i[0]].X) * (Points[i[3]].Y - Points[i[0]].Y) * (Points[i[2]].Z - Points[i[0]].Z)
				- (Points[i[2]].X - Points[i[0]].X) * (Points[i[1]].Y - Points[i[0]].Y) * (Points[i[3]].Z - Points[i[0]].Z)) / 6.0;
		}

        private HashSet<int> GetThetras(HashSet<int> points)
        {
            if (points == null)
                return null;
            HashSet<int> result = new HashSet<int>();
            foreach (int p in points)
                result.UnionWith(point2Thetra[p]);

            return result;
        }
	}

}
