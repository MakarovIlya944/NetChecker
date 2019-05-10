using System;
using System.Collections.Generic;
using System.Text;

namespace NetCheckApp
{
    public interface IGeometryDomain
    {
        bool IsExtern { get; }
        bool IsBoundary { get; }
        List<IGeometryElement> GeometryElements {
            get;
        }
        int MaterialNumber {
            get;
        }
    }

    public interface IGeometryElement
    {
        IGeometryData Geometry {
            get;
        }
    }

    public interface IElementCatalog
    {
        IMasterElement Master {
            get;
        }
    }

    public interface IMasterElement
    {
        ElementType ElementType {
            get;
        }
    }

    public enum ElementType
    {
        Point = 0,
        Segment = 1,
        Triangle = 2,
        Quadrangle = 3,
        Tetrahedron = 4,
        TriangularPrizm = 5, // Wedge
        Pyramid = 6,
        QuadrangularPrizm = 7 // Hex
    }

    public interface IGeometryData
    {
        int VertexCount {
            get;
        }
        int[] Indices {
            get;
        }
        IElementCatalog Metadata {
            get;
        }
        Vector3D this[int index] { get; }
    }

    public interface IMesh3D
    {
        List<Vector3D> Vertices {
            get;
        }
        IEnumerable<IGeometryDomain> AllDomains {
            get;
        }//домены и их границы
        Vector3D Center {
            get;
        }
        double Radius {
            get;
        }
    }


    public struct Vector3D : IEquatable<Vector3D>
    {
        public static readonly Vector3D Zero = new Vector3D(0, 0, 0);
        public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);
        public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);
        public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);
        public static readonly Vector3D[] Axes = { XAxis, YAxis, ZAxis };

        public double X {
            get;
        }
        public double Y {
            get;
        }
        public double Z {
            get;
        }

        public Vector3D(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }
        public double Distance(Vector3D b) => Distance(this, b);

        public double SqrDistance(Vector3D b) => SqrDistance(this, b);
        public static double Distance(Vector3D a, Vector3D b) => (a - b).Norm;

        public static double SqrDistance(Vector3D a, Vector3D b) {
            var diff = a - b;
            return diff * diff;
        }
        public double this[int i] {
            get {
                switch(i) {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }


        public double[] AsArray() => new[] { X, Y, Z };

        public double Norm => Math.Sqrt(X * X + Y * Y + Z * Z);

        public double MaxNorm => Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));

        public Vector3D Projection(Vector3D p) => (this * p) * p;

        public Vector3D Normalize() => this / Norm;

        public Vector3D Round(int digits) => new Vector3D(Math.Round(X, digits), Math.Round(Y, digits), Math.Round(Z, digits));

        public override string ToString() => $"Vec({X}, {Y}, {Z})";

        public override bool Equals(object obj) => obj is Vector3D && Equals((Vector3D)obj);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public bool Equals(Vector3D a) => a.X == X && a.Y == Y && a.Z == Z;

        public static bool TryParse(string line, out Vector3D res) {
            double x, y, z;
            var words = line.Split(new[] { ' ', '\t', ',', '(', ')', '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if(words[0] == "Vec") {
                if(words.Length != 4 || !double.TryParse(words[1], out x) || !double.TryParse(words[2], out y)
                    || !double.TryParse(words[3], out z)) {
                    res = Vector3D.Zero;
                    return false;
                }
                res = new Vector3D(x, y, z);
                return true;
            }
            if(words.Length != 3 || !double.TryParse(words[0], out x) || !double.TryParse(words[1], out y)
                || !double.TryParse(words[2], out z)) {
                res = Vector3D.Zero;
                return false;
            }

            res = new Vector3D(x, y, z);
            return true;
        }
        public static Vector3D Vec(double x, double y, double z) => new Vector3D(x, y, z);

        public static Vector3D Parse(string line) {
            Vector3D res;
            if(!TryParse(line, out res))
                throw new FormatException("Can't parse Vector3D!");
            return res;
        }

        #region Static operators

        
        public static Vector3D operator -(Vector3D a) => new Vector3D(-a.X, -a.Y, -a.Z);

        
        public static Vector3D operator +(Vector3D a) => a;

        
        public static double operator *(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        
        public static Vector3D operator *(double a, Vector3D b) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

        
        public static Vector3D operator *(Vector3D b, double a) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

        
        public static Vector3D operator /(Vector3D a, double v) => new Vector3D(a.X / v, a.Y / v, a.Z / v);

        
        public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        
        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        
        public static bool operator ==(Vector3D a, Vector3D b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        
        public static bool operator !=(Vector3D a, Vector3D b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

        
        public static Vector3D Cross(Vector3D v1, Vector3D v2) =>
            new Vector3D(v1.Y * v2.Z - v2.Y * v1.Z, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);

        
        public static double Mixed(Vector3D v1, Vector3D v2, Vector3D v3) =>
            (v1.Y * v2.Z - v2.Y * v1.Z) * v3.X + (v1.Z * v2.X - v1.X * v2.Z) * v3.Y + (v1.X * v2.Y - v1.Y * v2.X) * v3.Z;

        public static Vector3D Sum(Vector3D a, Vector3D b) => a + b;

        public static Vector3D Min(Vector3D a, Vector3D b) =>
            new Vector3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

        public static Vector3D Max(Vector3D a, Vector3D b) =>
            new Vector3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

        #endregion
    }
}
