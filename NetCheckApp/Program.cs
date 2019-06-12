
using System;
using System.Diagnostics;
using System.IO;

namespace NetCheckApp
{
    class Program
    {

		static void Main(string[] args)
        {
            string[] files = new string[10] { "1", "0.5", "0.25", "0.1", "0.09", "0.08", "0.07", "0.06", "0.05", "0.04" };
            int i = 1;
            //using(var file = new StreamWriter("D:\\Projects\\TelmaStaff\\researchConnectAddArray.txt")) {
            //    for(int i = 0; i < files.Length; i++) {
                    GmeshReader reader = new GmeshReader($"C:\\Users\\Krovo\\Desktop\\Новая папка\\error nets\\cube{files[i]}.msh");
                    var result = reader.Read();
                    Stopwatch stopWatch = new Stopwatch();
                    // Get the elapsed time as a TimeSpan value.


                    var fabric = new CheckerFabric();
                    INetChecker checker = (VolumeChecker)fabric.Create(CheckerMode.Объем);
                    checker.Load(result.Item2, result.Item1);
                    stopWatch.Start();
                    bool ans = checker.Check();
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    //file.WriteLine($"{files[i]} {(ts.Minutes * 60 + ts.Seconds) * 1000 + ts.Milliseconds}ms {ans} {result.Item2.Length} {result.Item1.Length}");
                    Console.WriteLine($"{files[i]} {(ts.Minutes * 60 + ts.Seconds) * 1000 + ts.Milliseconds}ms {ans} {result.Item2.Length} {result.Item1.Length}");
            //    }
            //}
            

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
