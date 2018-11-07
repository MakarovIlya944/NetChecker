using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetCheckApp
{
	struct Vector2D
	{
		double X { get; set; }
		double Y { get; set; }
	}

	public class MyQuadro
	{
		class QuadroTreeLeaf
		{
			public QuadroTreeLeaf[] children;
			public HashSet<int> container = new HashSet<int>();
			public Vector2D min, max;

			public QuadroTreeLeaf(Vector2D _min, Vector2D _max) { min = _min; max = _max; }
		}

		QuadroTreeLeaf root;
		List<Vector2D> HostArray;
		double minDist = 1E-0;
		int minElem = 1;

		public void AddElement(int v, QuadroTreeLeaf curLeaf)
		{

		}
	}
}