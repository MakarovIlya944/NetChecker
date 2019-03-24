using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCheckerFEM
{
    public class Program
    {
        static void Main()
        {
            NetGenerator a = new NetGenerator();
            a.Load(@"C:\Work\GMesh\cube.msh");
        }
    }
}
