using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetCheckApp
{
    public struct Vector2D
    {
        public Vector2D(double x, double y) { X = x; Y = y; }
        public Vector2D(Vector2D v) { X = v.X; Y = v.Y; }
        public double X { get; set; }
        public double Y { get; set; }

        public double Distance(Vector2D a) => Math.Sqrt((X - a.X) * (X - a.X) + (Y - a.Y) * (Y - a.Y));
    }

	public class MyQuadro
	{
		public class QuadroTreeLeaf
		{
			public QuadroTreeLeaf[] children;
			public HashSet<int> container = new HashSet<int>();
			public Vector2D min, max;
            public int level;

            public bool isConsist(Vector2D v) {
                bool flag = true;
                if (min.X < max.X)
                    flag &= v.X > min.X && v.X < max.X;
                else
                    flag &= v.X < min.X && v.X > max.X;
                if (min.Y < max.Y)
                    flag &= v.Y > min.Y && v.Y < max.Y;
                else
                    flag &= v.Y < min.Y && v.Y > max.Y;
                return flag;
            }

			public QuadroTreeLeaf(Vector2D _min, Vector2D _max, int _level) { level = _level; min = _min; max = _max; }
		}


        public MyQuadro(Vector2D a, Vector2D b)
        {
            root = new QuadroTreeLeaf(a, b, 0);
            HostArray = new List<Vector2D>();
        }

		QuadroTreeLeaf root;
		List<Vector2D> HostArray;
		double minDist = 1E-3;
		int minElem = 1;
        int maxLevel = 4;

		private void AddElement(int v, QuadroTreeLeaf curLeaf)
		{
            if (curLeaf.isConsist(HostArray[v]))
            {
                double dist = curLeaf.max.Distance(curLeaf.min);
                if (curLeaf != root && (curLeaf.container.Count < minElem || dist < minDist || curLeaf.level == maxLevel))
                    curLeaf.container.Add(v);
                else
                {
                    if (curLeaf.children == null)
                    {
                        curLeaf.children = new QuadroTreeLeaf[4];
                        curLeaf.children[0] = new QuadroTreeLeaf(new Vector2D(curLeaf.min),
                            new Vector2D((curLeaf.min.X + curLeaf.max.X) / 2, (curLeaf.min.Y + curLeaf.max.Y) / 2),curLeaf.level+1);
                        curLeaf.children[1] = new QuadroTreeLeaf(new Vector2D((curLeaf.min.X + curLeaf.max.X) / 2, (curLeaf.min.Y + curLeaf.max.Y) / 2),
                            new Vector2D(curLeaf.max), curLeaf.level + 1);
                        curLeaf.children[2] = new QuadroTreeLeaf(new Vector2D(curLeaf.min.X, curLeaf.max.Y),
                            new Vector2D(new Vector2D((curLeaf.min.X + curLeaf.max.X) / 2, (curLeaf.min.Y + curLeaf.max.Y) / 2)), curLeaf.level + 1);
                        curLeaf.children[3] = new QuadroTreeLeaf(new Vector2D(new Vector2D((curLeaf.min.X + curLeaf.max.X) / 2, (curLeaf.min.Y + curLeaf.max.Y) / 2)),
                            new Vector2D(curLeaf.max.X, curLeaf.min.Y), curLeaf.level + 1);
                    }

                    foreach (QuadroTreeLeaf el in curLeaf.children)
                        AddElement(v, el);
                    
                    foreach (int q in curLeaf.container)
                        foreach (QuadroTreeLeaf el in curLeaf.children)
                            AddElement(q, el);
                    curLeaf.container.Clear();
                }
            }
        }

        public void AddElement(Vector2D a)
        {
            HostArray.Add(a);
            AddElement(HostArray.Count - 1, root);
        }
	}
}