using System;
using System.Collections.Generic;
using System.Text;

namespace NetCheckApp
{
    /// <summary>
    ///0 1 2 - bot(0)
    ///0 1 3 - right(1)
    ///0 2 3 - left(2)
    ///1 2 3 - top(3)
    /// </summary>
    public class Thetra
    {
        public Thetra(int _a, int _b, int _c, int _d, int _mat)
        { p[0] = _a; p[1] = _b; p[2] = _c; p[3] = _d; material = _mat; }

        public Thetra(int[] mesh)
        {
            p[0] = mesh[0];
            p[1] = mesh[1];
            p[2] = mesh[2];
            p[3] = mesh[3];
            material = mesh[4];
        }

        public Thetra(string[] s)
        { p[0] = Convert.ToInt32(s[0]); p[1] = Convert.ToInt32(s[1]); p[2] = Convert.ToInt32(s[2]); p[3] = Convert.ToInt32(s[3]); material = Convert.ToInt32(s[4]); }

        /// <summary>
        /// Номера вершин
        /// </summary>
		public int[] p = new int[4] { 0, 0, 0, 0 };

        /// <summary>
        /// Номер материала
        /// </summary>
        public int material;

        /// <summary>
        /// Номера соседей
        /// при создании -1
        /// </summary>
        public int[] near = new int[4] { -1, -1, -1, -1 };

        public int[] GetSide(int side)
        {
            switch (side)
            {
                case 0:
                    return new int[3] { p[0], p[1], p[2] };
                case 1:
                    return new int[3] { p[0], p[1], p[3] };
                case 2:
                    return new int[3] { p[0], p[2], p[3] };
                case 3:
                    return new int[3] { p[1], p[2], p[3] };
                default:
                    return null;
            }
        }

        public int GetOpposite(int side)
        {
            switch (side)
            {
                case 0:
                    return p[3];
                case 1:
                    return p[2];
                case 2:
                    return p[1];
                case 3:
                    return p[0];
                default:
                    return 0;
            }
        }

        public override string ToString()
        {
            return $"{p[0]} {p[1]} {p[2]} {p[3]} | {material} | {near[0]} {near[1]} {near[2]} {near[3]}";
        }
    }
}
