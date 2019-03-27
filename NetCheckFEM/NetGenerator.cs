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

        List<Vector3D> points = new List<Vector3D>();
        List<Thetra> thetras = new List<Thetra>();

        public NetGenerator(string path)
        {
            Load(path);
        }

        void Load(string path)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                string str;
                while ((str = file.ReadLine()) != null)
                    if (str == "$Nodes")
                    {
                        while (str != "$EndNodes" && str.Split(' ').Length != 3) str = file.ReadLine();
                        if (str == "$EndNodes")
                            break;
                        var coords = file.ReadLine().Split(' ').Select(x => Double.Parse(x)).ToArray();
                        points.Add(new Vector3D(coords[0], coords[1], coords[2]));
                    }
                    else if(str == "$Elements")
                    {
                        while (str != "$EndElements" && str.Split(' ').Length != 5) str = file.ReadLine();
                        if (str == "$EndElements")
                            break;
                        var coords = file.ReadLine().Split(' ').Select(x => Int32.Parse(x)).ToArray();
                        thetras.Add(new Thetra(coords[0], coords[1], coords[2], coords[3], 0));
                    }
            }
        }
    }
}
