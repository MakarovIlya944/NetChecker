using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCheckApp;

namespace NetCheckerFEM
{
    class NetGenerator
    {

        struct Thetra
        {
            int a, b, c, d;
        }

        List<Vector3D> points = new List<Vector3D>();
        List<Thetra> thetras = new List<Thetra>();

        public NetGenerator()
        {

        }

        public void Load(string path)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                string str;
                while ((str = file.ReadLine()) != null)
                    if (str == "$Nodes")
                    {
                        while (str.Split(' ').Length != 3) str = file.ReadLine();
                        var coords = file.ReadLine().Split(' ').Select(x => Double.Parse(x)).ToArray();
                        points.Add(new Vector3D(coords[0], coords[1], coords[2]));
                    }
                    else if(str == "$Elements")
                    {
                        str = file.ReadLine();
                    }
            }
        }
    }
}
