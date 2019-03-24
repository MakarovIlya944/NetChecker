using System;


namespace NetCheckApp
{
    class Program
    {
		static void Main(string[] args)
        {
			Checker checker = new Checker();

            OctoTree tree = new OctoTree(new Vector3D(-10,-10,-10), new Vector3D(10,10,10));
            

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


            checker.Input();
			checker.MakeThetra();
			Console.WriteLine($"Value: {checker.FiguresValue} ==  {checker.AllValue()}");
			Console.WriteLine($"ConnectedСomponent: {checker.ConnectedСomponent()}");
			

            Console.WriteLine("Hello World!");
        }
    }
}
