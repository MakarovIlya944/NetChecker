using System;
using System.IO;
using System.Collections.Generic;

namespace NetCheckApp
{
    class Program
    {
		public class Checker
		{
			//0 1 2 - bot(0)
			//0 1 3 - right(1)
			//0 2 3 - left(2)
			//1 2 3 - top(3)
			public class Thetra
			{
				public Thetra(int _a, int _b, int _c, int _d, int _mat)
				{ p[0] = _a; p[1] = _b; p[2] = _c; p[3] = _d; material = _mat; }
				public Thetra(string[] s)
				{ p[0] = Convert.ToInt32(s[0]); p[1] = Convert.ToInt32(s[1]); p[2] = Convert.ToInt32(s[2]); p[3] = Convert.ToInt32(s[3]); material = Convert.ToInt32(s[4]); }
				//номера вершин
				public int[] p = new int[4]{0,0,0,0};
				//номер материала
				public int material;
				//номера соседей
				//при создании -1
				public int[] near = new int[4] { -1, -1, -1, -1 };

				//площадь
				//public double S;
				public override string ToString()
				{
					return string.Format("{0} {1} {2} {3} | {4} | {5} {6} {7} {8}", p[0], p[1], p[2], p[3], material, near[0], near[1], near[2], near[3]);
				}
			}

			public class Vec3
			{
				public double x { get; }
				public double y { get; }
				public double z { get; }
				public Vec3(double _x, double _y, double _z) { x = _x; y = _y; z = _z; }
				public Vec3(string[] s) { x = Convert.ToDouble(s[0]); y = Convert.ToDouble(s[1]); z = Convert.ToDouble(s[2]); }
				public override string ToString()
				{
					return string.Format("{0} {1} {2}", x, y, z);
				}
			}

			public List<Vec3> Points = new List<Vec3>();
			public List<Thetra> Figures = new List<Thetra>();
			public Dictionary<int, string> Names = new Dictionary<int, string>();
			//номера внешних тетраэдров
			public List<int> OutterThetra = new List<int>();
			//номера внешних вершин
			//public HashSet<int> OutterVertecies = new HashSet<int>();
			public double eps = 1E-10;
			public double FiguresValue = 0;
			private bool[] VisitedVertecies;
			private bool[] VisitedThetra;

			private double Distance(Vec3 v, Vec3 u)
			{
				return Math.Sqrt((v.x - u.x) * (v.x - u.x) + (v.y - u.y) * (v.y - u.y) + (v.z - u.z) * (v.z - u.z));
			}

			public bool CheckOneness(Vec3 v)
			{
				foreach (Vec3 u in Points)
					if (u == v)
						return false;
				return true;
			}

			public bool CheckNeighbourhood(Vec3 v)
			{
				foreach (Vec3 u in Points)
					if (Distance(u, v) < eps)
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
				
				Vec3 current;
				for (; i <= n; i++)
				{
					current = new Vec3(str_in[i].Split(' '));
					//проверка повторения
					//проверка близости
					if (CheckNeighbourhood(current))
						Points.Add(current);
					else
						Console.WriteLine("Точка слишком близко {0}", i);
				}
				
				n = i + Convert.ToInt32(str_in[i]);
				i++;

				//создание массива имен
				for (int j = n + 2, nj = Convert.ToInt32(str_in[j - 1]) + j - 1; j <= nj; j++)
					Names[Convert.ToInt32(str_in[j].Split(' ')[0])] = str_in[j].Split(' ')[1];

				VisitedThetra = new bool[n - i + 1];

				//создание массива Тетра
				for (; i <= n; i++)
				{
					//проверка имен
					if(CheckName(Convert.ToInt32(str_in[i].Split(' ')[4])))
						Figures.Add(new Thetra(str_in[i].Split(' ')));
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
						Console.WriteLine("Нулевой объём у тэтрадра #{0}", i);
					else
						FiguresValue += V;
				}
				
				//-проверка пересечения
				
				//проверка дыр
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
				foreach(Thetra Y in Figures)
					if(T != Y)
					{
						for (j = 0; j < 4; j++)
						{
							for (i = 0; i < 4 && Y.p[i] != T.p[j]; i++);
							if (i != 4)
							{
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
				/*int visited = 0;
				
				foreach(int var in _1(u))
					if(!VisitedVertecies[var])
					{
						visited++;
						VisitedVertecies[var] = true;
					}
				for (int i = 0, j; i < 4; i++)//соседние могут быть только по одной вершине
				{
					j = Figures[u].near[i];
					if (OutterThetra.Contains(j))
						visited += dfs(j);
				}
				return visited;*/

				int visited = 0;
				foreach(int a in _1(u))
				{
					if(!VisitedVertecies[a])
					{
						visited++;
						VisitedVertecies[a] = true;
						foreach(int b in _T(a))
						{
							if(!VisitedThetra[b])
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

			//нахождение общего объема
			public void AllValue()
			{
				
			}

			//нахождение объема одного
			public double Value(int u)
			{
				int[] i = Figures[u].p;

				return Math.Abs((Points[i[1]].x - Points[i[0]].x) * (Points[i[2]].y - Points[i[0]].y) * (Points[i[3]].z - Points[i[0]].z)
					+ (Points[i[3]].x - Points[i[0]].x) * (Points[i[1]].y - Points[i[0]].y) * (Points[i[2]].z - Points[i[0]].z)
					+ (Points[i[2]].x - Points[i[0]].x) * (Points[i[3]].y - Points[i[0]].y) * (Points[i[1]].z - Points[i[0]].z)
					- (Points[i[3]].x - Points[i[0]].x) * (Points[i[2]].y - Points[i[0]].y) * (Points[i[1]].z - Points[i[0]].z)
					- (Points[i[1]].x - Points[i[0]].x) * (Points[i[3]].y - Points[i[0]].y) * (Points[i[2]].z - Points[i[0]].z)
					- (Points[i[2]].x - Points[i[0]].x) * (Points[i[1]].y - Points[i[0]].y) * (Points[i[3]].z - Points[i[0]].z)) / 6.0;
			}
		}

		static void Main(string[] args)
        {
			Checker checker = new Checker();
			checker.Input();
			checker.MakeThetra();
			Console.WriteLine("Value: {0}", checker.FiguresValue);
			Console.WriteLine("ConnectedСomponent: {0}", checker.ConnectedСomponent());
			

            Console.WriteLine("Hello World!");
        }
    }
}
