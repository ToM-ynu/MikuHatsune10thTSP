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
            var filename = "qa194.tsp";
            if (args.Length != 0) filename = args[0];
            var data = Read(filename);
            //load finish 
            const int populationNumber = 200;
            const int cityNumber = 194;
            //make population population has 20 individuals.

            var fitness = new List<Pair<int, double>>();
            Random rand = new Random();
            var population = GeneticAlgorithm.Initialize(populationNumber, cityNumber, rand);


            Random[] paraRandom = new Random[10];
            var temp = Environment.TickCount;
            for (int i = 0; i < paraRandom.Length; i++)
            {
                paraRandom[i] = new Random(temp++);
            }

            var draw = new Draw(data);
            //draw.DrawMap(data, population[0]);
            var crossoverRate = 0.7;//Child is 70% its parent
            var mutationNum = 0.05; //mutation is happen @ 1%
            CalcFitness calc = new CalcFitness(data);
            for (int i = 0; i < population.Count; i++)
            {
                fitness.Add(new Pair<int, double>(i, calc.Calc(population[i])));
            }
            for (int i = 0; i < 1_000_000; i++)
            {
                population = GeneticAlgorithm.RunningGA(population, fitness, paraRandom, crossoverRate, mutationNum, cityNumber, calc);
  
                if (i % 1_000 == 0)
                {

                    for (int j = 0; j < fitness.Count; j++)
                    {
                        if (j % 40 == 0)
                            Console.Write("{0}:{1:e3}\t", fitness[j].First, fitness[j].Second);
                    }
                    Console.WriteLine();
                }
                if (i % 1_000*10 == 0)
                {
                    draw.DrawMap(data, population[0]);
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
}