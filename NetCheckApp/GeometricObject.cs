using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TelmaQuasar;
using static TelmaQuasar.Vector3D;
namespace TelmaQuasar
{
    public interface IGeometryObject
    {
        Vector3D Center { get; }
        double Radius { get; }
        IEnumerable<Vector3D> EnumerateVertices();
        /// <summary>
        /// У двумерных фигур IsPointInside всегда долна считать, что третья координата ноль
        /// </summary>
        bool IsPointInside(Vector3D p, double eps);
    }

    public interface IAffineObject
    {
        Vector3D WeightedSum(double[] weights);
    }

    public interface ISplitItem
    {
        int SplitPoint(Vector3D p, double eps);
        List<Vector3D> SplitLine(Vector3D p1, Vector3D p2, double eps);
        Vector3D LeftNormal(Vector3D p);
        bool IntersectWithSphere(Vector3D p, double r, double eps);
    }

    public static class GeometryObjectExtension
    {
        public static bool IsNear(this IGeometryObject obj, Vector3D p)
        {
            return (p - obj.Center).Norm < 1.5 * obj.Radius;
        }

        public static bool MayIntersectWithRay(this IGeometryObject obj, Vector3D A, Vector3D dir)
        {
            double t = ((obj.Center - A) * dir) / (dir * dir);

            return (obj.Center - A).Norm <= obj.Radius + Constants.GeometryEps ||
                (t > 0 && (A + t * dir - obj.Center).Norm <= obj.Radius + Constants.GeometryEps);
        }

        public static bool MayIntersectWithSphere(this IGeometryObject obj, Vector3D center, double radius)
        {
            return (obj.Center - center).Norm < obj.Radius + radius + Constants.GeometryEps;
        }

        public static bool MayIntersectWithSurface(this IGeometryObject obj, ISplitItem surface)
        {
            return surface.IntersectWithSphere(obj.Center, obj.Radius, obj.Radius * Constants.GeometryEps);
        }
        public static double Minx(this IGeometryObject obj) => obj.Center.X - obj.Radius;
        public static double Maxx(this IGeometryObject obj) => obj.Center.X + obj.Radius;
        public static double Miny(this IGeometryObject obj) => obj.Center.Y - obj.Radius;
        public static double Maxy(this IGeometryObject obj) => obj.Center.Y + obj.Radius;
        public static double Minz(this IGeometryObject obj) => obj.Center.Z - obj.Radius;
        public static double Maxz(this IGeometryObject obj) => obj.Center.Z + obj.Radius;
        public static Vector3D Max(this IGeometryObject obj) => Vector3D.Vec(obj.Maxx(), obj.Maxy(), obj.Maxz());
        public static Vector3D Min(this IGeometryObject obj) => Vector3D.Vec(obj.Minx(), obj.Miny(), obj.Minz());

    }
    /// <summary>
    /// Прямоугольный параллелелепипед
    /// </summary>
    [DataContract]
    public class GabaritObject : IGeometryObject
    {
        [DataMember]
        Vector3D XYZmin;
        [DataMember]
        Vector3D XYZmax;
        [OnDeserialized]
        void LoadEpilog(StreamingContext context) { check(); }
        void check()
        {
            if (XYZmin[0] > XYZmax[0])
            {
                double x = XYZmin[0];
                XYZmin = new Vector3D(XYZmax[0], XYZmin[1], XYZmin[2]);
                XYZmax = new Vector3D(x, XYZmax[1], XYZmax[2]);
            }

            if (XYZmin[1] > XYZmax[1])
            {
                double y = XYZmin[1];
                XYZmin = new Vector3D(XYZmin[0], XYZmax[1], XYZmin[2]);
                XYZmax = new Vector3D(XYZmax[0], y, XYZmax[2]);
            }

            if (XYZmin[2] > XYZmax[2])
            {
                double z = XYZmin[2];
                XYZmin = new Vector3D(XYZmin[0], XYZmin[1], XYZmax[2]);
                XYZmax = new Vector3D(XYZmax[0], XYZmax[1], z);
            }
        }
        public GabaritObject()
        {
            Reset();
        }
        public GabaritObject(Vector3D minp, Vector3D maxp)
        {
            XYZmax = maxp;
            XYZmin = minp;
            check();
        }
        public GabaritObject(IGeometryObject obj)
        {
            var cent = obj.Center;
            var r = obj.Radius;

            XYZmax = Vec(cent.X + r, cent.Y + r, cent.Z + r);
            XYZmin = Vec(cent.X - r, cent.Y - r, cent.Z - r);
            check();
        }

        public GabaritObject(double xmin, double ymin, double zmin, double xmax, double ymax, double zmax)
        {
            XYZmin = new Vector3D(xmin, ymin, zmin);
            XYZmax = new Vector3D(xmax, ymax, zmax);
            check();
        }
        public Vector3D MinPoint => XYZmin;
        public Vector3D MaxPoint => XYZmax;
        public double minx => XYZmin[0];
        public double maxy => XYZmax[1];
        public double maxx => XYZmax[0];
        public double miny => XYZmin[1];
        public double minz => XYZmin[2];
        public double maxz => XYZmax[2];
        public Vector3D Center => (XYZmin + XYZmax) / 2;
        public double Radius => CircumscribedRadius;
        public double InscribedRadius
        {
            get
            {
                return Math.Max(Math.Max(Width, Height), Depth) / 2;
            }
        }
        public double CircumscribedRadius
        {
            get
            {
                return (XYZmax - XYZmin).Norm / 2;
            }
        }
        public bool IsPointInside(Vector3D p, double eps) => In(p, eps);
        public bool In(Vector3D p, double eps)
        {
            return minx - eps <= p[0] && maxx + eps >= p[0] &&
                   miny - eps <= p[1] && maxy + eps >= p[1] &&
                   minz - eps <= p[2] && maxz + eps >= p[2];

        }
        public void Include(Vector3D p)
        {
            XYZmin = new Vector3D(Math.Min(XYZmin[0], p[0]), Math.Min(XYZmin[1], p[1]), Math.Min(XYZmin[2], p[2]));
            XYZmax = new Vector3D(Math.Max(XYZmax[0], p[0]), Math.Max(XYZmax[1], p[1]), Math.Max(XYZmax[2], p[2]));
        }
        public void Include(IEnumerable<Vector3D> pt)
        {
            foreach (var p in pt) Include(p);
        }
        public void Include(GabaritObject gab)
        {
            if (!gab.IsInvalidWidth)
            {
                ExtentByX(gab.MinPoint.X); ExtentByX(gab.MaxPoint.X);
            }
            if (!gab.IsInvalidHeight)
            {
                ExtentByY(gab.MinPoint.Y); ExtentByY(gab.MaxPoint.Y);
            }
            if (!gab.IsInvalidDepth)
            {
                ExtentByZ(gab.MinPoint.Z); ExtentByZ(gab.MaxPoint.Z);
            }
        }

        void ExtentByX(double x)
        {
            if (XYZmin.X > x) XYZmin = new Vector3D(x, XYZmin.Y, XYZmin.Z);
            if (XYZmax.X < x) XYZmax = new Vector3D(x, XYZmax.Y, XYZmax.Z);
        }
        void ExtentByY(double y)
        {
            if (XYZmin.Y > y) XYZmin = new Vector3D(XYZmin.X, y, XYZmin.Z);
            if (XYZmax.Y < y) XYZmax = new Vector3D(XYZmax.X, y, XYZmax.Z);
        }
        void ExtentByZ(double z)
        {
            if (XYZmin.Z > z) XYZmin = new Vector3D(XYZmin.X, XYZmin.Y, z);
            if (XYZmax.Z < z) XYZmax = new Vector3D(XYZmax.X, XYZmax.Y, z);
        }
        public void SetMinMax(Vector3D minp, Vector3D maxp)
        {
            XYZmin = minp;
            XYZmax = maxp;
            check();
        }
        void Reset()
        {
            XYZmin = new Vector3D(double.MaxValue, double.MaxValue, double.MaxValue);
            XYZmax = new Vector3D(double.MinValue, double.MinValue, double.MinValue);
        }
        public bool IsEmptyWidth => XYZmin.X >= XYZmax.X;
        public bool IsEmptyHeight => XYZmin.Y >= XYZmax.Y;
        public bool IsEmptyDepth => XYZmin.Z >= XYZmax.Z;
        public bool IsInvalidWidth => XYZmin.X > XYZmax.X || double.IsInfinity(XYZmax.X) || double.IsInfinity(XYZmin.X);
        public bool IsInvalidHeight => XYZmin.Y > XYZmax.Y || double.IsInfinity(XYZmax.Y) || double.IsInfinity(XYZmin.Y);
        public bool IsInvalidDepth => XYZmin.Z > XYZmax.Z || double.IsInfinity(XYZmax.Z) || double.IsInfinity(XYZmin.Z);
        public double Width => XYZmax[0] - XYZmin[0];
        public double Height => XYZmax[1] - XYZmin[1];
        public double Depth => XYZmax[2] - XYZmin[2];
        public IEnumerable<Vector3D> EnumerateVertices()
        {
            yield return XYZmin;
            yield return XYZmax;
        }
    }
    public class TelmaGraphicScaleMessage : Exception
    {
        public IGeometryObject Gabarit { get; }
        public TelmaGraphicScaleMessage(IGeometryObject gabarit, string message) : base(message)
        {
            Gabarit = gabarit;
        }
    }
	
    [DataContract]
    public class LineObject3D //!< отрезок прямой
    {
        [DataMember]
        Vector3D p1 = new Vector3D(0, 0, 0);
        [DataMember]
        Vector3D p2 = new Vector3D(0, 0, 0);
        double len = 0;
        Vector3D vec = new Vector3D(0, 0, 0);
        [OnDeserialized]
        void LoadEpilog(StreamingContext context)
        {
            Reset();
        }
        void Reset()
        {
            vec = p2 - p1;
            len = vec.Norm;
            vec /= len;
        }

        public LineObject3D(Vector3D a, Vector3D b)
        {
            p1 = a;
            p2 = b;
            Reset();
        }
        public LineObject3D() { }
        public Vector3D Begin => p1;
        public Vector3D End => p2;
        public Vector3D Center => (p1 + p2) / 2.0;
        public Vector3D this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return p1;
                    case 1: return p2;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
        public double Len => len;

        public double SqrDistance(Vector3D p) => Proection(p).SqrDistance(p);
        //Geometry
        public Vector3D Proection(Vector3D p)
        {
            double coor = (p - p1) * vec;
            if (coor < 0) coor = 0;
            if (coor > len) coor = len;
            return p1 + coor * vec;
        }
        public Vector3D ParameterCurve(double t) { return p1 * (1 - t) + p2 * t; }
    }
    
    public class ProjectionPlane
    {
        protected Vector3D XVector { get; }
        protected Vector3D YVector { get; }
        protected Vector3D ZVector { get; }
        protected Vector3D Zero { get; set; }
        public ProjectionPlane(Vector3D Zero, Vector3D X, Vector3D Y, Vector3D Z)
        {
            this.Zero = Zero;
            XVector = X;
            YVector = Y;
            ZVector = Z;
        }
        public ProjectionPlane()
        {
            Zero = Vec(0, 0, 0);
            XVector = Vec(1, 0, 0);
            YVector = Vec(0, 1, 0);
            ZVector = Vec(0, 0, 1);
        }
        public Vector3D ToLocal(Vector3D p)
        {
            Vector3D pp = p - Zero;
            return Vec(pp * XVector, pp * YVector, pp * ZVector);
        }
		public Vector3D ToGlobal(Vector3D p) => p.X * XVector + p.Y * YVector + p.Z * ZVector + Zero;
        public Vector3D ToGlobalVec(Vector3D p) => p.X * XVector + p.Y * YVector + p.Z * ZVector;
        public Vector3D ToLocalVec(Vector3D p) => Vec(p * XVector, p * YVector, p * ZVector);
    }
}
