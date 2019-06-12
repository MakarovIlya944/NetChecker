
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NetCheckApp {
    class Program {

        static void Main(string[] args) {
            string[] files = new string[10] { "1", "0.5", "0.25", "0.1", "0.09", "0.08", "0.07", "0.06", "0.05", "0.04" };
            int i = 0;
            bool b = false;
            if(b)
                using(var file = new StreamWriter("D:\\Projects\\TelmaStaff\\researchVolumeWithoutCenter.txt")) {
                    for(; i < files.Length; i++) {
                        GmeshReader reader = new GmeshReader($"C:\\Users\\Krovo\\Desktop\\Новая папка\\error nets\\cube{files[i]}.msh");
                        var result = reader.Read();
                        Stopwatch stopWatch = new Stopwatch();
                        // Get the elapsed time as a TimeSpan value.
                        INetChecker checker = (VolumeChecker)new CheckerFabric().Create(CheckerMode.Объем);

                        // remove center
                        //int m = 0, j = 0;
                        //Vector3D c = new Vector3D(0.5, 0.5, 0.5);
                        //for(; m < result.Item2.Length; m++)
                        //    if(Vector3D.Distance(result.Item2[j], c) > Vector3D.Distance(result.Item2[m], c))
                        //        j = m;

                        //var thetras = result.Item1.Where(x => x.p.All(y => y != j));

                        checker.Load(result.Item2, result.Item1);
                        stopWatch.Start();
                        bool ans = checker.Check();
                        stopWatch.Stop();
                        TimeSpan ts = stopWatch.Elapsed;
                        file.WriteLine($"{files[i]} {(ts.Minutes * 60 + ts.Seconds) * 1000 + ts.Milliseconds}ms {ans} {result.Item2.Length} {result.Item1.Length}");
                        Console.WriteLine($"{files[i]} {(ts.Minutes * 60 + ts.Seconds) * 1000 + ts.Milliseconds}ms {ans} {result.Item2.Length} {result.Item1.Length}");
                    }
                }
            else {
                GmeshReader reader = new GmeshReader($"C:\\Users\\Krovo\\Desktop\\Новая папка\\error nets\\cube0.5StandAlone.msh");
                var result = reader.Read();
                //int m = 0, j = 0;
                //Vector3D c = new Vector3D(0.5, 0.5, 0.5);
                //for(; m < result.Item2.Length; m++)
                //    if(Vector3D.Distance(result.Item2[j], c) > Vector3D.Distance(result.Item2[m], c))
                //        j = m;
                //var thetras = result.Item1.Where(x => x.p.All(y => y != j));
                Stopwatch stopWatch = new Stopwatch();
                // Get the elapsed time as a TimeSpan value.


                INetChecker checker = (VolumeChecker)new CheckerFabric().Create(CheckerMode.Объем);
                checker.Load(result.Item2, result.Item1);
                stopWatch.Start();
                bool ans = checker.Check();
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Console.WriteLine($"{files[i]} {(ts.Minutes * 60 + ts.Seconds) * 1000 + ts.Milliseconds}ms {ans} {result.Item2.Length} {result.Item1.Length}");
            }
        }
    }
}
