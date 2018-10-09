using System;

namespace TelmaQuasar
{
    public static class Constants
    {
        public const double c = 299792.458;
        public const double mu0 = 4 * Math.PI * 1e-7;
        public const double eps0 = 1 / (c * c * mu0);
        public const double GeometryEps = 1e-8;
        public const double SqrGeometryEps = 1e-15;
        public const int BoundaryDrawIntervalNumber = 16;
    }
}
