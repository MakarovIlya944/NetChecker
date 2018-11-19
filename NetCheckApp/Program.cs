using System;


namespace NetCheckApp
{
    class Program
    {
		static void Main(string[] args)
        {
			Checker checker = new Checker();

            MyQuadro tree = new MyQuadro(new Vector2D(-10,-10), new Vector2D(10,10));
            tree.AddElement(new Vector2D(1, 1));
            tree.AddElement(new Vector2D(1, 1.1));
            tree.AddElement(new Vector2D(1, 1.01));
            tree.AddElement(new Vector2D(2, 1));
            tree.AddElement(new Vector2D(2, 1.01));
            tree.AddElement(new Vector2D(2, 1.1));
            tree.AddElement(new Vector2D(1, 4));
            tree.AddElement(new Vector2D(5, 1));


            /*checker.Input();
			checker.MakeThetra();
			Console.WriteLine("Value: {0}", checker.FiguresValue);
			Console.WriteLine("ConnectedСomponent: {0}", checker.ConnectedСomponent());
			

            Console.WriteLine("Hello World!");*/
        }
    }
}
