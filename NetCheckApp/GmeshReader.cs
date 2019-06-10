using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetCheckApp {
    public class GmeshReader {
        private string path = "";

        public GmeshReader(string _path) {
            path = _path;
        }

        public (Thetra[], Vector3D[]) Read() {

            Thetra[] resultThetras;
            Vector3D[] resultVectors; 
            string cur;
            string[] curNumbers;
            int numEntityBlocks, numNodes, numElements;
            int[] mesh = new int[4];
            using(var file = new StreamReader(path)) {
                cur = file.ReadLine();

                while(cur != "$Nodes")
                    cur = file.ReadLine();

                cur = file.ReadLine();
                curNumbers = cur.Split(' ');
                numEntityBlocks = Int32.Parse(curNumbers[0]);
                numNodes = Int32.Parse(curNumbers[1]);

                resultVectors = new Vector3D[numNodes];

                for(int i = 0, ind = 0; i < numEntityBlocks; i++) {
                    cur = file.ReadLine();
                    curNumbers = cur.Split(' ');
                    numNodes = Int32.Parse(curNumbers[3]);
                    for(int j = 0; j < numNodes; j++) file.ReadLine();
                    for(int j = 0; j < numNodes; j++) resultVectors[ind++] = new Vector3D(file.ReadLine());
                }
                while(cur != "$Elements")
                    cur = file.ReadLine();

                cur = file.ReadLine();
                curNumbers = cur.Split(' ');
                numEntityBlocks = Int32.Parse(curNumbers[0]);
                numElements = Int32.Parse(curNumbers[1]);

                resultThetras = new Thetra[numElements];

                for(int i = 0, ind = 0; i < numEntityBlocks; i++) {
                    cur = file.ReadLine();
                    curNumbers = cur.Split(' ');
                    if(curNumbers[0] != "3") {
                        for(int j = 0; j < Int32.Parse(curNumbers[3]); j++) file.ReadLine();
                        continue;
                    }
                    numNodes = Int32.Parse(curNumbers[3]);
                    for(int j = 0; j < numNodes; j++) {
                        curNumbers = file.ReadLine().Split(" ");
                        for(int k = 0; k < 3; k++)
                            mesh[k] = Int32.Parse(curNumbers[k+1]) - 1;
                        mesh[3] = 0;
                        resultThetras[ind++] = new Thetra(mesh);
                    }
                }
            }

            return (resultThetras, resultVectors);
        }
    }
}
