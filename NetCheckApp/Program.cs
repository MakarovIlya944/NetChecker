using System;
using System.Collections.Generic;
using System.IO;

namespace NetCheckApp
{
    class Program
    {
        static (List<Vector3D>, List<Thetra>) Input(string path) {
            List<Vector3D> points = new List<Vector3D>();
            List<Thetra> figures = new List<Thetra>();
            using(var reader = new StreamReader(path)) {
                int n = Int32.Parse(reader.ReadLine());
                for(int i = 0; i < n; i++)
                    points.Add(Vector3D.Parse(reader.ReadLine()));

                n = Int32.Parse(reader.ReadLine());
                for(int i = 0; i < n; i++)
                    figures.Add(new Thetra(reader.ReadLine().Split(" ")));
            }
            return (points, figures);
        }

		static void Main(string[] args)
        {
            /*tree.AddElement(new Vector3D(1, 1,1));
            tree.AddElement(new Vector3D(1, 1.1,10));
            tree.AddElement(new Vector3D(1, 1.01,1));
            tree.AddElement(new Vector3D(2, 1,1.11));
            tree.AddElement(new Vector3D(2, 1.01,10));
            tree.AddElement(new Vector3D(2, 1.1,1.1));
            tree.AddElement(new Vector3D(2, 1.1000000001,1.1));
            tree.AddElement(new Vector3D(5, 1,10));
            var a = tree.Find(1);
            int afsd = 3;*/

            var input = Input("in.txt");
            var fabric = new CheckerFabric();
            INetChecker checker;

            Console.WriteLine("Start: ConnectChecker");
            checker = (ConnectChecker)fabric.Create(CheckerMode.CONNECT_COMPONENT);
            checker.Load(input.Item1, input.Item2);
            Console.WriteLine($"ConnectChecker: {checker.Check()}");
            Console.WriteLine("End: ConnectChecker\n\n");

            Console.WriteLine("Start: VolumeChecker");
            checker = (VolumeChecker)fabric.Create(CheckerMode.VOLUME);
            checker.Load(input.Item1, input.Item2);
            Console.WriteLine($"VolumeChecker: {checker.Check()}");
            Console.WriteLine("End: VolumeChecker\n\n");

            Console.WriteLine("Start: FEMChecker");
            checker = (FEMChecker)fabric.Create(CheckerMode.FEM);
            checker.Load(input.Item1, input.Item2);
            Console.WriteLine($"FEMChecker: {checker.Check()}");
            Console.WriteLine("End: FEMChecker\n\n");


            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
