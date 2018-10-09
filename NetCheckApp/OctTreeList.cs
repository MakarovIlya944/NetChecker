using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Telma.Geometry;
using TelmaQuasar;
using static TelmaQuasar.Vector3D;
using static System.Math;
using System.Linq;
using System.Collections;

namespace Telma.Geometry
{
    [DataContract]
    public class OctTreeList<T> : IFindableCollection<T> where T: IGeometryObject
    {
        class OctTreeLeaf
        {
            public OctTreeLeaf[] children;
            public HashSet<int> container=new HashSet<int>();
            public GabaritObject gabarit;
            public OctTreeLeaf(GabaritObject gab)
            {
                gabarit = gab;
            }
            public bool HasIntersection(T sz)
            {
                if(gabarit.minx>sz.Maxx()) return false;
                if (gabarit.miny > sz.Maxy()) return false;
                if (gabarit.minz > sz.Maxz()) return false;
                if (gabarit.maxx < sz.Minx()) return false;
                if (gabarit.maxy < sz.Miny()) return false;
                if (gabarit.maxz < sz.Minz()) return false;
                return true;
            }

            public bool IsPointInside(Vector3D p, double eps)
            {
                throw new NotImplementedException();
            }
        }
        OctTreeLeaf root;
        [DataMember]
        int MaxLeafSize;
        [DataMember]
        IList<T> HostArray;

        [OnDeserialized]
        void LoadEpilog(StreamingContext context)
        {
            RebuildTree();
        }

        public int Count => HostArray.Count;

        public bool IsReadOnly => HostArray.IsReadOnly;

        public T this[int index] { get => HostArray[index]; set => HostArray[index] = value; }

        bool NeedRebuild = false;
        private void AddElement(OctTreeLeaf leaf, int ind, T obj)
        {
            if (leaf.children != null)
            {
                if(leaf.children.All(l=>l.HasIntersection(obj))) // бессмысленно переписывать, содержится везде
                {
                    leaf.container.Add(ind);
                    if (leaf.gabarit.minx > obj.Minx() ||
                        leaf.gabarit.miny > obj.Miny()||
                        leaf.gabarit.minz > obj.Minz() ||
                    leaf.gabarit.maxx < obj.Maxx()||
                    leaf.gabarit.maxy > obj.Maxy() ||
                    leaf.gabarit.maxz > obj.Maxz()
                    ) NeedRebuild=true;
                }
                else
                {
                    foreach (var child in leaf.children.Where(l=>l.HasIntersection(obj))) AddElement(child, ind, obj);
                }
            }
            else
            {
                leaf.container.Add(ind);
                leaf.gabarit.Include(obj.Min());
                leaf.gabarit.Include(obj.Max());
                if (leaf.container.Count >= MaxLeafSize)
                {
                    double minx = leaf.gabarit.minx;
                    double miny = leaf.gabarit.miny;
                    double minz = leaf.gabarit.minz;
                    double maxx = leaf.gabarit.maxx;
                    double maxy = leaf.gabarit.maxy;
                    double maxz = leaf.gabarit.maxz;
                    double cx = (minx + maxx) / 2;
                    double cy = (miny + maxy) / 2;
                    double cz = (minz + maxz) / 2;

                    leaf.children = new OctTreeLeaf[8];
                    leaf.children[0] = new OctTreeLeaf(new GabaritObject(minx, miny, minz, cx, cy, cz));
                    leaf.children[1] = new OctTreeLeaf(new GabaritObject(cx, miny, minz, maxx, cy, cz));
                    leaf.children[2] = new OctTreeLeaf(new GabaritObject(minx, cy, minz, cx, maxy, cz));
                    leaf.children[3] = new OctTreeLeaf(new GabaritObject(cx, cy, minz, maxx, maxy, cz));
                    leaf.children[4] = new OctTreeLeaf(new GabaritObject(minx, miny, cz, cx, cy, maxz));
                    leaf.children[5] = new OctTreeLeaf(new GabaritObject(cx, miny, cz, maxx, cy, maxz));
                    leaf.children[6] = new OctTreeLeaf(new GabaritObject(minx, cy, cz, cx, maxy, maxz));
                    leaf.children[7] = new OctTreeLeaf(new GabaritObject(cx, cy, cz, maxx, maxy, maxz));
                    var indices = leaf.container;
                    leaf.container = new HashSet<int>();
                    foreach (var i in indices)
                    {
                        AddElement(leaf, i, HostArray[i]);
                    }
                }
            }
        }
        private void RemoveElement(OctTreeLeaf leaf, int ind, T obj)
        {
            if (leaf.container.Contains(ind))
            {
                leaf.container.Remove(ind);
            }
            else
            {
                if(leaf.children != null)
                    foreach(var l in leaf.children.Where(c=>c.HasIntersection(obj))) RemoveElement(l, ind, obj);
            }
        }
        private IEnumerable<int> Find(OctTreeLeaf leaf,ISplitItem surface,double eps)
        {
            if (!leaf.gabarit.MayIntersectWithSurface(surface))
                yield break;
            foreach (var elem in leaf.container)
                if (HostArray[elem].MayIntersectWithSurface(surface))
                    yield return elem;
            if (leaf.children != null)
            {
                foreach(var c in leaf.children)
                    foreach (var res in Find(c, surface, eps)) yield return res;
            }
        }
        private IEnumerable<int> Find(OctTreeLeaf leaf, Vector3D p, double eps)
        {
            if(!leaf.gabarit.In(p,eps))yield break;
            foreach (var i in leaf.container)
            {
                if (HostArray[i].Center.Distance(p)< HostArray[i].Radius+eps && HostArray[i].IsPointInside(p,eps)) yield return i;
            }
            if (leaf.children != null)
            {
                foreach (var c in leaf.children)
                    foreach (var res in Find(c, p, eps)) yield return res;
            }
        }
        public OctTreeList(GabaritObject Max=null,int _MaxLeafSize = 100)
        {
            MaxLeafSize = _MaxLeafSize;
            HostArray = new List<T>();
            if (Max == null) Max = new GabaritObject();
            root = new OctTreeLeaf(Max);
        }
        public void RebuildTree()
        {
            var full = new GabaritObject();
            foreach (var gab in HostArray)
            {
                full.Include(gab.Min());
                full.Include(gab.Max());
            }
            root = new OctTreeLeaf(full);
            for (int i = 0; i < HostArray.Count; i++) AddElement(root,i,HostArray[i]);
            NeedRebuild = false;
        }

        public IEnumerable<T> Find(Vector3D p, double eps)
        {
            return Find(root, p, eps).Select(m=>HostArray[m]);
        }
        public IEnumerable<int> FindIndices(Vector3D p, double eps)
        {
            return Find(root, p, eps);
        }

        public int IndexOf(T item)
        {
            return HostArray.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            var obj = HostArray[index];
            RemoveElement(root, index, obj);
            if(index != HostArray.Count-1)
            {
                HostArray[index] = HostArray[HostArray.Count - 1];
                obj = HostArray[index];
                RemoveElement(root, HostArray.Count - 1, obj);
                AddElement(root, index, obj);
            }
            HostArray.RemoveAt(HostArray.Count - 1);
            if (NeedRebuild) RebuildTree();
        }

        public void Add(T item)
        {
            HostArray.Add(item);            
            AddElement(root, HostArray.Count - 1, item);
            if (NeedRebuild) RebuildTree();
        }
        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                HostArray.Add(item);
                AddElement(root, HostArray.Count - 1, item);
            }
            if (NeedRebuild) RebuildTree();
        }
        public void Clear()
        {
            HostArray.Clear();
            root = new OctTreeLeaf(root.gabarit);
        }
        public bool Contains(T item)
        {
            return HostArray.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            HostArray.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var i=HostArray.IndexOf(item);
            if (i >= 0) { RemoveAt(i); return true; }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return HostArray.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return HostArray.GetEnumerator();
        }

        public IEnumerable<T> Find(ISplitItem surface, double eps =Constants.GeometryEps)
        {
            return Find(root, surface, eps).Select(i=>HostArray[i]);
        }
    }
}
