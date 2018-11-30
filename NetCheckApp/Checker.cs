using System;
using System.Collections.Generic;
using System.Text;
using TelmaQuasar;
using System.IO;
using Telma.Geometry;

namespace NetCheckApp
{
	public class Checker
	{
		public List<Vector3D> Points = new List<Vector3D>();
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
            OctoTree tree = new OctoTree(new Vector3D(), new Vector3D());
			Vector3D current;
			for (; i <= n; i++)
			{
				Vector3D.TryParse(str_in[i], out current);

                tree.AddElement(current);

				//проверка повторения
				//проверка близости
				if (CheckNeighbourhood(current))//Проверять через октодерева
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
				if (CheckName(Convert.ToInt32(str_in[i].Split(' ')[4])))
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

		//нахождение общего объема
		public void AllValue()
		{
			//по всем слоям(dz)
			//выбрать слой(zc,octotree)
			//посчитать площадь
			//v+=si*dz
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
	}

}
