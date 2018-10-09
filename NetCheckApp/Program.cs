using System;


namespace NetCheckApp
{
    class Program
    {
		static void Main(string[] args)
        {
			Checker checker = new Checker();
			checker.Input();
			checker.MakeThetra();
			Console.WriteLine("Value: {0}", checker.FiguresValue);
			Console.WriteLine("ConnectedСomponent: {0}", checker.ConnectedСomponent());
			

            Console.WriteLine("Hello World!");
        }
    }
}
