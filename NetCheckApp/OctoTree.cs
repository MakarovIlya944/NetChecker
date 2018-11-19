using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TelmaQuasar;

namespace NetCheckApp
{
	class OctoTree
	{
		/*class OctoTreeLeaf
		{
			public OctoTreeLeaf[] children;
			public HashSet<int> container = new HashSet<int>();
			public Vector3D min, max;

			public OctoTreeLeaf(Vector3D _min, Vector3D _max) { min = _min; max = _max; }

			public bool IsPointInside(Vector3D p, double eps)
			{
				throw new NotImplementedException();
			}

			public void Include(Vector3D v)
			{

			}
		}
		OctoTreeLeaf root;
		int MaxLeafSize;
		IList<Vector3D> HostArray;
		double MinDist = 1E-2;

		public int Count => HostArray.Count;

		public bool IsReadOnly => HostArray.IsReadOnly;

		public Vector3D this[int index] { get => HostArray[index]; set => HostArray[index] = value; }

		private void AddElement(OctoTreeLeaf leaf, int ind, Vector3D obj)
		{
			if (leaf.children != null)
			{
				if (leaf.children.All(l => l.HasIntersection(obj))) // бессмысленно переписывать, содержится везде
				{
					leaf.container.Add(ind);
					if (leaf.gabarit.minx > obj.Minx() ||
						leaf.gabarit.miny > obj.Miny() ||
						leaf.gabarit.minz > obj.Minz() ||
					leaf.gabarit.maxx < obj.Maxx() ||
					leaf.gabarit.maxy > obj.Maxy() ||
					leaf.gabarit.maxz > obj.Maxz()
					) NeedRebuild = true;
				}
				else
				{
					foreach (var child in leaf.children.Where(l => l.HasIntersection(obj))) AddElement(child, ind, obj);
				}
			}
			else
			{
				leaf.container.Add(ind);
				if (leaf.container.Count >= MaxLeafSize)
				{
					double minx = leaf.min.X;
					double miny = leaf.min.Y;
					double minz = leaf.min.Z;
					double maxx = leaf.max.X;
					double maxy = leaf.max.Y;
					double maxz = leaf.max.Z;
					double cx = (minx + maxx) / 2;
					double cy = (miny + maxy) / 2;
					double cz = (minz + maxz) / 2;

					leaf.children = new OctoTreeLeaf[8];
					leaf.children[0] = new OctoTreeLeaf(new Vector3D(minx, miny, minz),new Vector3D( cx, cy, cz));
					leaf.children[1] = new OctoTreeLeaf(new Vector3D(cx, miny, minz), new Vector3D(maxx, cy, cz));
					leaf.children[2] = new OctoTreeLeaf(new Vector3D(minx, cy, minz), new Vector3D(cx, maxy, cz));
					leaf.children[3] = new OctoTreeLeaf(new Vector3D(cx, cy, minz), new Vector3D(maxx, maxy, cz));
					leaf.children[4] = new OctoTreeLeaf(new Vector3D(minx, miny, cz), new Vector3D(cx, cy, maxz));
					leaf.children[5] = new OctoTreeLeaf(new Vector3D(cx, miny, cz), new Vector3D(maxx, cy, maxz));
					leaf.children[6] = new OctoTreeLeaf(new Vector3D(minx, cy, cz), new Vector3D(cx, maxy, maxz));
					leaf.children[7] = new OctoTreeLeaf(new Vector3D(cx, cy, cz), new Vector3D(maxx, maxy, maxz));
					var indices = leaf.container;
					leaf.container = new HashSet<int>();
					foreach (var i in indices)
					{
						AddElement(leaf, i, HostArray[i]);
					}
				}
			}
		}*/
	}
}