using System;
using TelmaQuasar;

namespace NetCheckApp
{
    class Program
    {
		static void Main(string[] args)
        {
			Checker checker = new Checker();

            OctoTree tree = new OctoTree(new Vector3D(-10,-10,-10), new Vector3D(10,10,10));
            tree.AddElement(new Vector3D(1, 1,1));
            tree.AddElement(new Vector3D(1, 1.1,2));
            tree.AddElement(new Vector3D(1, 1.01,1));
            tree.AddElement(new Vector3D(2, 1,1));
            tree.AddElement(new Vector3D(2, 1.01,1));
            tree.AddElement(new Vector3D(2, 1.1,1));
            tree.AddElement(new Vector3D(1, 4,1));
            tree.AddElement(new Vector3D(5, 1,1));
            var a = tree.Find(new Vector3D(1, 1,1));
            int afsd = 3;


            /*checker.Input();
			checker.MakeThetra();
			Console.WriteLine("Value: {0}", checker.FiguresValue);
			Console.WriteLine("ConnectedСomponent: {0}", checker.ConnectedСomponent());
			

            Console.WriteLine("Hello World!");*/
        }
    }
}
