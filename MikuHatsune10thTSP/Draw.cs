using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MikuHatsune10thTSP
{
    class Draw
    {
        Pair<Pair<double, double>, Pair<double, double>> BoundlyBox;
        double scale;
        public void DrawMap(List<City> data, int[] individual)
        {
            var filename = MakeNewFile();
            var fileStream = new FileStream(filename, FileMode.Create);
            using (var writer = new StreamWriter(fileStream))
            {
                DrawingSVG.Init(writer);
                for (int i = 0; i < individual.Length - 1; i++)
                {

                    DrawWire(data[individual[i] - 1], data[individual[i + 1] - 1], writer);
                }
                DrawWire(data[individual[0]-1], data[individual[individual.Length - 1]-1], writer);
                foreach (var item in data)
                {
                    DrawCircle(item, writer);
                }
                DrawingSVG.End(writer);
            }

        }
        private void DrawWire(City left, City right, StreamWriter writer)
        {
            int leftX, leftY, rightX, rightY;
            leftX = (int)((left.XAxis - BoundlyBox.First.First) * scale + 50);
            leftY = (int)((left.YAxis - BoundlyBox.First.Second) * scale + 50);
            rightX = (int)((right.XAxis - BoundlyBox.First.First) * scale + 50);
            rightY = (int)((right.YAxis - BoundlyBox.First.Second) * scale + 50);
            DrawingSVG.MakeLine(leftX, 1200 - leftY, rightX, 1200 - rightY, writer);
        }
        private void DrawCircle(City city, StreamWriter writer)
        {
            var xAxis = (int)((city.XAxis - BoundlyBox.First.First) * scale + 50);
            var yAxis = (int)((city.YAxis - BoundlyBox.First.Second) * scale + 50);
            DrawingSVG.MakeCircle(xAxis, 1200 - yAxis, 4, writer);
        }
        static string MakeNewFile()
        {
            string filename = "RealNewFile.html";
            string stCurrentDir = System.IO.Directory.GetCurrentDirectory();
            int i = 0;
            for (i = 1; File.Exists(stCurrentDir + "\\" + filename); i++)
            {
                filename = "RealNewFile" + i.ToString() + ".html";
            }
            return filename;
        }
        private void GetLenge(List<City> data)
        {
            double maxX = 0, minX = double.MaxValue, maxY = 0, minY = double.MaxValue;
            foreach (var item in data)
            {
                if (item.XAxis < minX) minX = item.XAxis;
                else if (maxX < item.XAxis) maxX = item.XAxis;

                if (item.YAxis < minY) minY = item.YAxis;
                else if (maxY < item.YAxis) maxY = item.YAxis;

            }
            var left = new Pair<double, double>(minX, minY);
            var right = new Pair<double, double>(maxX, maxY);
            BoundlyBox = new Pair<Pair<double, double>, Pair<double, double>>(left, right);
        }
        private void Normalization()
        {
            var lengeData = BoundlyBox;
            const int Ylimit = 1000;
            var yScale = Ylimit / (lengeData.Second.Second - lengeData.First.Second);
            this.scale = yScale;
        }

        public Draw(List<City> data)
        {
            GetLenge(data);
            Normalization();
        }
    }
}
