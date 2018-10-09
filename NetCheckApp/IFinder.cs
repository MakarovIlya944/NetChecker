using System;
using System.Collections.Generic;
using System.Text;
using TelmaQuasar;
using static TelmaQuasar.Vector3D;
using static System.Math;

namespace Telma.Geometry
{
    public interface IFinderByPoint<T> where T : IGeometryObject//! Базовый класс для поиска попадания точки в объект
    {
        IEnumerable<T> Find(Vector3D p, double eps = Constants.GeometryEps);
        IEnumerable<T> Find(ISplitItem surface, double eps = Constants.GeometryEps);
    }
    public interface IFindableCollection<T>: IFinderByPoint<T>, IReadOnlyList<T> where T : IGeometryObject
    {

    }

    public interface IFinderByPoint2D<T> : IFinderByPoint<T> where T : IGeometryObject//! Базовый класс для поиска попадания 2D точки в объект
    {
        CoordinateSystem2D CS { get; }
        IEnumerable<T> Find(Vector3D p, double eps);
    }
    public interface IFindableCollection2D<T> : IFinderByPoint2D<T>, IFindableCollection<T> where T : IGeometryObject
    {

    }
}