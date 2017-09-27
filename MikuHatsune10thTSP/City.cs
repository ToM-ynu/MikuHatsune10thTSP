using System;

namespace MikuHatsune10thTSP
{
    public class City
    {
        private int number;
        private double xAxis;
        private double yAxis;

        public int Number { get => number; set => number = value; }
        public double XAxis { get => xAxis; set => xAxis = value; }
        public double YAxis { get => yAxis; set => yAxis = value; }

        private void SetCity(int number, double y, double x)
        {
            Number = number;
            XAxis = x;
            YAxis = y;
        }
        public City(string dataLine)
        {
            var temp = dataLine.Split(' ');
            SetCity(int.Parse(temp[0]), double.Parse(temp[1]), double.Parse(temp[2]));

        }
        public void WriteLine()
        {
            Console.WriteLine("{0} {1} {2}", Number, XAxis, YAxis);

        }
    }
}