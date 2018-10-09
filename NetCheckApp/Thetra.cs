using System;
using System.Collections.Generic;
using System.Text;

namespace NetCheckApp
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
		public int[] p = new int[4] { 0, 0, 0, 0 };
		//номер материала
		public int material;
		//номера соседей
		//при создании -1
		public int[] near = new int[4] { -1, -1, -1, -1 };

		//площадь
		//public double S;
		public override string ToString()
		{
			return $"{p[0]} {p[1]} {p[2]} {p[3]} | {material} | {near[0]} {near[1]} {near[2]} {near[3]}";
		}
	}
}
