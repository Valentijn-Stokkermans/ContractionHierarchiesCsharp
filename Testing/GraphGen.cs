using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Testing
{
    public class GraphGen
    {
        int numberOfVertices = 0;

        public GraphGen(int nbVertices) { numberOfVertices = nbVertices; }
        public void gen()
        {
            string filePath = @"C:\Users\valentijn\source\repos\GraphTest\Valentijn\data\new_graph.csv";
            var csv = new StringBuilder();
            Random rnd = new Random();

            var from = 0;
            var to1 = 1;
            var weight = rnd.Next(1, 100);
            var newLine = string.Format("{0},{1},{2}", from, to1, weight);
            csv.AppendLine(newLine);

            from = 0;
            var to2 = random_except_list(numberOfVertices, new HashSet<int> { from, to1 });
            weight = rnd.Next(1, 100);
            newLine = string.Format("{0},{1},{2}", from, to2, weight);
            csv.AppendLine(newLine);

            from = 0;
            var to3 = random_except_list(numberOfVertices, new HashSet<int> { from, to1, to2 });
            weight = rnd.Next(1, 100);
            newLine = string.Format("{0},{1},{2}", from, to3, weight);
            csv.AppendLine(newLine);

            for (int i = 1; i < numberOfVertices - 1; i++)
            {
                from = i;
                to1 = i - 1;
                weight = rnd.Next(1, 100);
                newLine = string.Format("{0},{1},{2}", from, to1, weight);
                csv.AppendLine(newLine);

                from = i;
                to2 = i + 1;
                weight = rnd.Next(1, 100);
                newLine = string.Format("{0},{1},{2}", from, to2, weight);
                csv.AppendLine(newLine);

                from = i;
                to3 = random_except_list(numberOfVertices, new HashSet<int> { from, to1, to2 }); ;
                weight = rnd.Next(1, 100);
                newLine = string.Format("{0},{1},{2}", from, to3, weight);
                csv.AppendLine(newLine);
            }

            from = numberOfVertices - 1;
            to1 = numberOfVertices - 2;
            weight = rnd.Next(1, 100);
            newLine = string.Format("{0},{1},{2}", from, to1, weight);
            csv.AppendLine(newLine);

            from = numberOfVertices - 1;
            to2 = random_except_list(numberOfVertices, new HashSet<int> { from, to1 });
            weight = rnd.Next(1, 100);
            newLine = string.Format("{0},{1},{2}", from, to2, weight);
            csv.AppendLine(newLine);

            from = numberOfVertices - 1;
            to3 = random_except_list(numberOfVertices, new HashSet<int> { from, to1, to2 });
            weight = rnd.Next(1, 100);
            newLine = string.Format("{0},{1},{2}", from, to3, weight);
            csv.AppendLine(newLine);

            //after your loop
            File.WriteAllText(filePath, csv.ToString());
        }

        public void genMatrix()
        {
            string filePath = @"C:\Users\valentijn\source\repos\GraphTest\Valentijn\data\new_graph.csv";
            var csv = new StringBuilder();

            string newLine = string.Format("from, to, weight");
            csv.AppendLine(newLine);

            int size = (int)Math.Sqrt(numberOfVertices);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int from = i * size + j;
                    int weight = 1;
                    // top
                    if (i != 0)
                    {
                        int to = (i - 1) * size + j;
                        newLine = string.Format("{0},{1},{2}", from, to, weight);
                        csv.AppendLine(newLine);
                    }
                    // bottom
                    if (i != size - 1)
                    {
                        int to = (i + 1) * size + j;
                        newLine = string.Format("{0},{1},{2}", from, to, weight);
                        csv.AppendLine(newLine);
                    }
                    // left
                    if (j != 0)
                    {
                        int to = i * size + (j - 1);
                        newLine = string.Format("{0},{1},{2}", from, to, weight);
                        csv.AppendLine(newLine);
                    }
                    // right
                    if (j != size - 1)
                    {
                        int to = i * size + (j + 1);
                        newLine = string.Format("{0},{1},{2}", from, to, weight);
                        csv.AppendLine(newLine);
                    }
                }
            }
            File.WriteAllText(filePath, csv.ToString());
        }

        public int random_except_list(int n, HashSet<int> x)
        {
            Random r = new Random();
            int result;
            do
            {
                result = r.Next(0, n);
            } while (x.Contains(result));
            return result;
        }
    }

}
