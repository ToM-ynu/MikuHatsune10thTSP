using System;
using System.Collections.Generic;
using System.IO;

namespace MikuHatsune10thTSP
{
    class Program
    {
        static void Main(string[] args)
        {
            //load data
            var filename = "ja9847.txt";
            if (args.Length != 0) filename = args[0];
            var data = Read(filename);
            //load finish 
            const int individualNumber = 20;
            //make population population has 20 individuals.

            var fitness = new Pair<int, double>[individualNumber];
            for (int i = 0; i < fitness.Length; i++)
            {
                fitness[i] = new Pair<int, double>();
            }
            Random rand = new Random();
            var population = GeneticAlgorithm.Initialize(data.Count, rand);


            for (int i = 0; i < 1000000; i++)
            {
                CalcFitness.Calc(fitness, population, data);
                population = GeneticAlgorithm.MakeChildren(population, fitness, rand);
                if (i % 1000 == 0)
                {
                    for (int j = 0; j < fitness.Length / 3; j++)
                    {
                        Console.Write("{0}:{1:e2}\t", fitness[j].First, fitness[j].Second);
                    }
                    Console.WriteLine();
                }
            }


        }

        static List<City> Read(string filename)
        {
            List<City> dataSet = new List<City>();

            FileStream fileStream = new FileStream("dataSet/" + filename, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string temp;

                for (int i = 0; i < 7; i++)
                {
                    reader.ReadLine();
                }
                while (true)
                {
                    temp = reader.ReadLine();
                    if (temp == "EOF") break;
                    var city = new City(temp);
                    dataSet.Add(city);
                }
            }
            return dataSet;
        }
    }
    public class City
    {
        private int number;
        private double xAxis;
        private double yAxis;

        public int Number { get => number; set => number = value; }
        public double XAxis { get => xAxis; set => xAxis = value; }
        public double YAxis { get => yAxis; set => yAxis = value; }

        private void SetCity(int number, double x, double y)
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
    public class Pair<T, U>
    {
        T first;
        U second;
        public Pair()
        {

        }
        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get => first; set => first = value; }
        public U Second { get => second; set => second = value; }

        internal object Clone()
        {
            return MemberwiseClone();
        }
    }
}