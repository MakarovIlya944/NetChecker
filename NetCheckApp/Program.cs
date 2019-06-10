
using System;

namespace NetCheckApp
{
    class Program
    {

		static void Main(string[] args)
        {
            GmeshReader reader = new GmeshReader("D:\\Projects\\TelmaStaff\\cube.msh");
            var result = reader.Read();


            var fabric = new CheckerFabric();
            INetChecker checker = (VolumeChecker)fabric.Create(CheckerMode.Объем);
            checker.Load(result.Item2, result.Item1);
            bool ans = checker.Check();
            Console.WriteLine($"VolumeChecker: {ans}");

            //Console.WriteLine("Start: ConnectChecker");
            //checker = (ConnectChecker)fabric.Create(CheckerMode.CONNECT_COMPONENT);
            //checker.Load(input.Item1, input.Item2);
            //Console.WriteLine($"ConnectChecker: {checker.Check()}");
            //Console.WriteLine("End: ConnectChecker\n\n");

            //Console.WriteLine("Start: VolumeChecker");
            //checker = (VolumeChecker)fabric.Create(CheckerMode.VOLUME);
            //checker.Load(input.Item1, input.Item2);
            //Console.WriteLine($"VolumeChecker: {checker.Check()}");
            //Console.WriteLine("End: VolumeChecker\n\n");

            //Console.WriteLine("Start: FEMChecker");
            //checker = (FEMChecker)fabric.Create(CheckerMode.FEM);
            //checker.Load(input.Item1, input.Item2);
            //Console.WriteLine($"FEMChecker: {checker.Check()}");
            //Console.WriteLine("End: FEMChecker\n\n");


            //Console.WriteLine("Hello World!");
            //Console.ReadKey();
        }
    }
}
