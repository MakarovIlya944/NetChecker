using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TelmaQuasar;

namespace NetCheckApp
{
    public class OctoTree
    {
        public class OctoTreeLeaf
        {
            public OctoTreeLeaf[] children;
            public HashSet<int> container = new HashSet<int>();
            public Vector3D min, max;
            public int level;

            public bool isConsist(Vector3D v)
            {
                bool flag = true;
                if (min.X < max.X)
                    flag &= v.X >= min.X && v.X <= max.X;
                else
                    flag &= v.X <= min.X && v.X >= max.X;
                if (min.Y < max.Y)
                    flag &= v.Y >= min.Y && v.Y <= max.Y;
                else
                    flag &= v.Y <= min.Y && v.Y >= max.Y;
                if (min.Z < max.Z)
                    flag &= v.Z >= min.Z && v.Z <= max.Z;
                else
                    flag &= v.Z <= min.Z && v.Z >= max.Z;
                return flag;
            }

            public bool isZConsist(double z)
            {
                if (min.Z < max.Z)
                    return min.Z <= z && z <= max.Z;
                else
                    return z >= max.Z && min.Z >= z;
            }

            public OctoTreeLeaf(Vector3D _min, Vector3D _max, int _level) { level = _level; min = _min; max = _max; }

            public override string ToString() => $"L={level} min={min} max={max}";
        }


        public OctoTree(Vector3D a, Vector3D b)
        {
            root = new OctoTreeLeaf(a, b, 0);
            HostArray = new List<Vector3D>();
        }

        OctoTreeLeaf root;
        List<Vector3D> HostArray;
        public double minDist = 1E-3;
        int minElem = 1;
        int maxLevel = 4;
        public bool tooClose = false;
        public enum treeStopFactors {d = 1,c,dc,l,dl,cl,dcl};
        public treeStopFactors checkStop = treeStopFactors.d;

        private void AddElement(int v, OctoTreeLeaf curLeaf)
        {
            if (curLeaf.isConsist(HostArray[v]))
            {
                double dist = curLeaf.max.Distance(curLeaf.min);
                if (curLeaf != root && checkDist && dist < minDist)
                    curLeaf.container.Add(v);
                else if (curLeaf != root && (!checkLevel || curLeaf.level == maxLevel))
                    curLeaf.container.Add(v);
                else if(curLeaf != root && (!checkCount || curLeaf.container.Count < minElem))
                    curLeaf.container.Add(v);
                else
                {
                    if (curLeaf.children == null)
                    {
                        curLeaf.children = new OctoTreeLeaf[8];
                        Vector3D C = (curLeaf.max + curLeaf.min) / 2;
                        curLeaf.children[0] = new OctoTreeLeaf(curLeaf.min,
                           C, curLeaf.level + 1);
                        curLeaf.children[1] = new OctoTreeLeaf(C,
                            curLeaf.max, curLeaf.level + 1);
                        curLeaf.children[2] = new OctoTreeLeaf(new Vector3D(curLeaf.min.X, curLeaf.min.Y, curLeaf.max.Z),
                            C, curLeaf.level + 1);
                        curLeaf.children[3] = new OctoTreeLeaf(new Vector3D(curLeaf.min.X, curLeaf.max.Y, curLeaf.min.Z),
                             C, curLeaf.level + 1);
                        curLeaf.children[4] = new OctoTreeLeaf(new Vector3D(curLeaf.max.X, curLeaf.min.Y, curLeaf.min.Z),
                            C, curLeaf.level + 1);
                        curLeaf.children[5] = new OctoTreeLeaf(new Vector3D(curLeaf.max.X, curLeaf.max.Y, curLeaf.min.Z),
                            C, curLeaf.level + 1);
                        curLeaf.children[6] = new OctoTreeLeaf(new Vector3D(curLeaf.max.X, curLeaf.min.Y, curLeaf.max.Z),
                            C, curLeaf.level + 1);
                        curLeaf.children[7] = new OctoTreeLeaf(new Vector3D(curLeaf.min.X, curLeaf.max.Y, curLeaf.max.Z),
                            C, curLeaf.level + 1);
                    }

                    foreach (OctoTreeLeaf el in curLeaf.children)
                        AddElement(v, el);

                    foreach (int q in curLeaf.container)
                        foreach (OctoTreeLeaf el in curLeaf.children)
                            AddElement(q, el);
                    curLeaf.container.Clear();
                }
            }
        }

        private OctoTreeLeaf DeepFind(int v, OctoTreeLeaf curLeaf)
        {
            if (curLeaf.isConsist(HostArray[v]))
                if (curLeaf.children == null)
                    return curLeaf;
                else
                    foreach (OctoTreeLeaf el in curLeaf.children)
                    {
                        OctoTreeLeaf a = DeepFind(v, el);
                        if (a != null)
                            return a;
                    }
            return null;
        }

        private List<OctoTreeLeaf> DeepZFind(double z, OctoTreeLeaf curLeaf)
        {
            List<OctoTreeLeaf> result = new List<OctoTreeLeaf>();
            if (curLeaf.isZConsist(z))
            {
                if (curLeaf.children == null)
                    result.Add(curLeaf);
                else
                    foreach (OctoTreeLeaf el in curLeaf.children)
                    {
                        List<OctoTreeLeaf> a = DeepZFind(z, el);
                        if (a != null)
                            result = result.Union(a).ToList<OctoTreeLeaf>();
                    }
                return result;
            }
            return null;
        }

        public HashSet<Vector3D> Find(Vector3D v)
        {
            HashSet<Vector3D> res = new HashSet<Vector3D>();
            foreach (int a in DeepFind(HostArray.IndexOf(v), root).container)
                res.Add(HostArray[a]);

            return res;
        }

        public void AddElement(Vector3D a)
        {
            HostArray.Add(a);
            AddElement(HostArray.Count - 1, root);
        }

        /// <summary>
        /// Поиск точек ближайщих к плоскости параллельной плокости z = 0
        /// </summary>
        /// <param name="z">Высота</param>
        /// <returns>Множество точек ближайших к плоскости</returns>
        public HashSet<Vector3D> Find(double z)
        {
            HashSet<Vector3D> result = new HashSet<Vector3D>();
            foreach (OctoTreeLeaf el in DeepZFind(z, root))
                foreach (int v in el.container)
                    result.Add(HostArray[v]);
            return result;
        }
    }
}