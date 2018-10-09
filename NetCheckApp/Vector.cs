using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TelmaQuasar
{
    public enum CoordinateSystem2D { CS2D_XY, CS2D_RZ, CS2D_RFi };
    public enum AngleMeasureUnits { amuRadians = 0, amuDegrees = 1 };
	
    public struct Vector3D : IEquatable<Vector3D>
    {
        public static readonly Vector3D Zero = new Vector3D(0, 0, 0);
        public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);
        public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);
        public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);
        public static readonly Vector3D[] Axes = { XAxis, YAxis, ZAxis };

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /*public Vector3D(ReadOnlySpan<double> arr)
        {
#if DEBUG
            if (arr.Length != 3) throw new ArgumentException();
#endif
            X = arr[0];
            Y = arr[1];
            Z = arr[2];
        }*/
        public double Distance(Vector3D b) => Distance(this, b);

        public double SqrDistance(Vector3D b) => SqrDistance(this, b);
        public static double Distance(Vector3D a, Vector3D b) => (a - b).Norm;

        public static double SqrDistance(Vector3D a, Vector3D b)
        {
            var diff = a - b;
            return diff * diff;
        }
        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new IndexOutOfRangeException();
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

        public static bool TryParse(string line, out Vector3D res)
        {
            double x, y, z;
            var words = line.Split(new[] { ' ', '\t', ',', '(', ')', '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (words[0] == "Vec")
            {
                if (words.Length != 4 || !double.TryParse(words[1], out x) || !double.TryParse(words[2], out y)
                    || !double.TryParse(words[3], out z))
                {
                    res = Vector3D.Zero;
                    return false;
                }
                res = new Vector3D(x, y, z);
                return true;
            }
            if (words.Length != 3 || !double.TryParse(words[0], out x) || !double.TryParse(words[1], out y)
                || !double.TryParse(words[2], out z))
            {
                res = Vector3D.Zero;
                return false;
            }

            res = new Vector3D(x, y, z);
            return true;
        }
        public static Vector3D Vec(double x, double y, double z) => new Vector3D(x, y, z);

        public static Vector3D Parse(string line)
        {
            Vector3D res;
            if (!TryParse(line, out res))
                throw new FormatException("Can't parse Vector3D!");
            return res;
        }

        #region Static operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator -(Vector3D a) => new Vector3D(-a.X, -a.Y, -a.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator +(Vector3D a) => a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double operator *(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator *(double a, Vector3D b) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator *(Vector3D b, double a) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator /(Vector3D a, double v) => new Vector3D(a.X / v, a.Y / v, a.Z / v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3D a, Vector3D b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3D a, Vector3D b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D Cross(Vector3D v1, Vector3D v2) =>
            new Vector3D(v1.Y * v2.Z - v2.Y * v1.Z, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Mixed(Vector3D v1, Vector3D v2, Vector3D v3) =>
            (v1.Y * v2.Z - v2.Y * v1.Z) * v3.X + (v1.Z * v2.X - v1.X * v2.Z) * v3.Y + (v1.X * v2.Y - v1.Y * v2.X) * v3.Z;

        public static Vector3D Sum(Vector3D a, Vector3D b) => a + b;

        public static Vector3D Min(Vector3D a, Vector3D b) =>
            new Vector3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

        public static Vector3D Max(Vector3D a, Vector3D b) =>
            new Vector3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

        #endregion

        #region EqualityComparer

        private class EqualityComparer : IEqualityComparer<Vector3D>
        {
            public int Digits { get; set; }

            public bool Equals(Vector3D v1, Vector3D v2)
            {
                return v1.Round(Digits) == v2.Round(Digits);
            }

            public int GetHashCode(Vector3D obj)
            {
                return obj.Round(Digits).GetHashCode();
            }
        }

        public static IEqualityComparer<Vector3D> CreateComparer(int digits = 7)
        {
            return new EqualityComparer { Digits = digits };
        }

        internal object GetCoords()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class VectorExtensions
    {
        public static Vector3D WeightedSum(this IEnumerable<Vector3D> vectors, IEnumerable<double> weights)
        {
            return vectors.Zip(weights, (a, b) => a * b).Aggregate(Vector3D.Sum);
        }

        public static Vector3D WeightedSum(this Vector3D[] vectors, double[] weights)
        {
            double x = 0, y = 0, z = 0;
            for (int i = 0; i < vectors.Length; i++)
            {
                x += vectors[i].X * weights[i];
                y += vectors[i].Y * weights[i];
                z += vectors[i].Z * weights[i];
            }
            return new Vector3D(x, y, z);
        }
      
        public static Vector3D CenterMass(this IEnumerable<Vector3D> vectors)
        {
            return (vectors.Aggregate(Vector3D.Sum) / vectors.Count());
        }
		
        public static Vector3D CenterBox(this IEnumerable<Vector3D> vectors)
        {
            Vector3D min = vectors.Aggregate(vectors.First(), Vector3D.Min);
            Vector3D max = vectors.Aggregate(vectors.First(), Vector3D.Max);
            return (min + max) / 2;
        }

        public static Vector3D Center(this IEnumerable<Vector3D> vectors)
        {
            return vectors.CenterMass();
        }

        public static double Radius(this IEnumerable<Vector3D> vectors)
        {
            var c = vectors.Center();
            return vectors.Max(v => (v - c).Norm);
        }

        public static Func<Vector3D, int> SplitBox(this IEnumerable<Vector3D> vectors)
        {
            Vector3D min = vectors.Aggregate(Vector3D.Min);
            Vector3D max = vectors.Aggregate(Vector3D.Max);
            Vector3D center = (min + max) / 2;
            double diameter = Vector3D.Axes.Max(a => (max - min) * a);
            var v = Vector3D.Axes.Where(a => (max - min) * a > diameter / 2).ToArray();
            return p => v.Select((x, i) => (p - center) * x >= 0 ? 1 << i : 0).Sum();
        }

        public static Func<Vector3D, int> SplitSphere(this IEnumerable<Vector3D> vectors)
        {
            Vector3D min = vectors.Aggregate(vectors.First(), Vector3D.Min);
            Vector3D max = vectors.Aggregate(vectors.First(), Vector3D.Max);
            Vector3D center = (min + max) / 2;

            var e = new Vector3D[3];
            e[0] = vectors.Aggregate(vectors.First() - center,
                                     (acc, curr) => (curr - center).Norm > acc.Norm ? curr - center : acc);

            if (e[0].Norm > 1e-10)
            {
                Func<Vector3D, Vector3D> pr1 = x => x - (x * e[0]) / (e[0] * e[0]) * e[0];
                e[1] = vectors.Aggregate(pr1(vectors.First() - center),
                                           (acc, curr) => pr1(curr - center).Norm > acc.Norm ? pr1(curr - center) : acc);
                if (e[1].Norm > 1e-10)
                {
                    Func<Vector3D, Vector3D> pr2 = x => x - (x * e[0]) / (e[0] * e[0]) * e[0] - (x * e[1]) / (e[1] * e[1]) * e[1];
                    e[2] = vectors.Aggregate(pr2(vectors.First() - center),
                                               (acc, curr) => pr2(curr - center).Norm > acc.Norm ? pr2(curr - center) : acc);
                }
            }

            var v = e.Where(a => 2 * a.Norm > e[0].Norm).ToArray();
            return p => v.Select((x, i) => (p - center) * x >= 0 ? 1 << i : 0).Sum();
        }

        public static double[,] JacobiEigenValues(double[,] a)
        {
            int n = a.GetLength(0);
            var u = new double[3, 3];
            for (int i = 0; i < n; i++)
                u[i, i] = 1;

            do
            {
                int maxi = 1, maxj = 0;
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < i; j++)
                        if (Math.Abs(a[i, j]) > Math.Abs(a[maxi, maxj]))
                        {
                            maxi = i;
                            maxj = j;
                        }
                if (Math.Abs(a[maxi, maxj]) < 1e-3) break;

                double phi = 0.5 * Math.Atan2(2 * a[maxi, maxj], a[maxi, maxi] - a[maxj, maxj]);
                double cos = Math.Cos(phi);
                double sin = Math.Sin(phi);

                for (int i = 0; i < n; i++)
                {
                    double x = a[i, maxi];
                    double y = a[i, maxj];
                    a[i, maxi] = cos * x - sin * y;
                    a[i, maxj] = sin * x + cos * y;
                }
                for (int i = 0; i < n; i++)
                {
                    double x = a[maxi, i];
                    double y = a[maxj, i];
                    a[maxi, i] = cos * x - sin * y;
                    a[maxj, i] = sin * x + cos * y;
                }

                for (int i = 0; i < n; i++)
                {
                    double x = u[maxi, i];
                    double y = u[maxj, i];
                    u[maxi, i] = cos * x - sin * y;
                    u[maxj, i] = sin * x + cos * y;
                }
            } while (true);

            return u;
        }

        public static Func<Vector3D, int> Principial(this IEnumerable<Vector3D> vectors)
        {
            Vector3D mean = vectors.Center();
            int n = vectors.Count();
            var C = new double[3, 3];
            double a = 1.0 / (n - 1);
            foreach (var v in vectors.Select(v => (v - mean)))
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        C[i, j] += a * (v * Vector3D.Axes[i]) * (v * Vector3D.Axes[j]);
            }
            double trace = 0;
            for (int i = 0; i < 3; i++) trace += C[i, i];
            var u = JacobiEigenValues(C);

            var eigenvalues = Enumerable.Range(0, 3).Select(
                i => new { lambda = C[i, i], x = new Vector3D(u[i, 0], u[i, 1], u[i, 2]) }).
                OrderByDescending(e => e.lambda).TakeWhile(next => next.lambda > trace / 3).Select(r => r.x).ToArray();

            return p => eigenvalues.Select((x, i) => (p - mean) * x >= 0 ? 1 << i : 0).Sum();


            //Random r = new Random(12345);
            //Vector3D x = new Vector3D(r.NextDouble(), r.NextDouble(), r.NextDouble()).Normalize();
            //Vector3D dx;
            //do
            //{
            //    dx.X = C[0, 0] * x.X + C[0, 1] * x.Y + C[0, 2] * x.Z;
            //    dx.Y = C[1, 0] * x.X + C[1, 1] * x.Y + C[1, 2] * x.Z;
            //    dx.Z = C[2, 0] * x.X + C[2, 1] * x.Y + C[2, 2] * x.Z;
            //    dx /= dx.Norm;
            //    dx -= x;
            //    x += dx;
            //} while (dx.Norm > 1e-2);
            //return x;
        }
    }
}
