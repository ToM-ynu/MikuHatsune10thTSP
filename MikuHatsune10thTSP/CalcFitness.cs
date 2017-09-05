using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;
namespace MikuHatsune10thTSP
{
    static class CalcFitness
    {

        public static void Calc(Pair<int, double>[] fitness, int[][] population, List<City> data)
        {
            for (int i = 0; i < fitness.Length; i++)
            {
                fitness[i] = new Pair<int, double>(i, Calc(population, data, i));
            }
            for (int i = 0; i < fitness.Length; i++)
            {
                for (int k = i + 1; k < fitness.Length; k++)
                {
                    if (fitness[i].Second < fitness[k].Second)
                    {
                        var temp = (Pair<int, double>)fitness[i].Clone();
                        fitness[i] = (Pair<int, double>)fitness[k].Clone();
                        fitness[k] = (Pair<int, double>)temp.Clone();
                    }
                }
            }
        }
        public static double Calc(int[][] population, List<City> data, int individual)
        {

            var ans = 0.0;
            var temp = population[individual];
            for (int j = 0; j < temp.Length - 1; j++)
            {
                ans += Pow(City2City(data[temp[j] - 1], data[temp[j + 1] - 1]), 3);

            }
            ans += City2City(data[temp[0] - 1], data[temp[temp.Length - 1] - 1]);
            return (1.0 / ans);


        }
        private static double City2City(City left, City right)
        {

            return Sqrt(Pow(Abs(left.XAxis - right.XAxis), 2)
                + Pow(Abs(left.YAxis - right.YAxis), 2));
        }
    }
}
