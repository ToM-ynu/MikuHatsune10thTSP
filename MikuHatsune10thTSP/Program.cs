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
            var filename = "dj38.tsp";
            if (args.Length != 0) filename = args[0];
            var data = Read(filename);
            //load finish 
            const int populationNumber = 200;
            const int maxPopulationNumber = 700;

            //make population population has 20 individuals.

            var fitness = new(int, double)[maxPopulationNumber];
            Random rand = new Random();
            var population = new int[2][][];
            GeneticAlgorithm.Initialize(population, data.Count, maxPopulationNumber, rand);
            var temp = Environment.TickCount;
            Random[] paraRandom = new Random[Environment.ProcessorCount];
            for (int i = 0; i < paraRandom.Length; i++)
            {
                paraRandom[i] = new Random(temp++);
            }

            var draw = new Draw(data);
            //draw.DrawMap(data, population[0]);
            var crossoverRate = 0.7;//Child is 70% its parent
            var mutationNum = 0.05; //mutation is happen @ 1%
            CalcFitness calc = new CalcFitness(data);
            for (int i = 0; i < population.GetLength(0); i++)
            {
                fitness[i] = (i, calc.Calc(population[0][i]));
            }
            for (int i = 0; i < 1_000_000; i++)
            {
                GeneticAlgorithm.RunningGA(population, fitness, paraRandom, crossoverRate, mutationNum, data.Count, calc);

                if (i % 1_000 == 0)
                {

                    for (int j = 0; j < populationNumber; j++)
                    {
                        if (j % 40 == 0)
                            Console.Write("{0}:{1:e3}\t", fitness[j].Item1, fitness[j].Item2);
                    }
                    Console.WriteLine();
                }
                if (i % 1_000 * 10 == 0)
                {
                    draw.DrawMap(data, population[0][0]);
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
                while (reader.Peek() > -1)
                {
                    temp = reader.ReadLine();
                    dataSet.Add(new City(temp));
                }
            }
            return dataSet;
        }
    }
}