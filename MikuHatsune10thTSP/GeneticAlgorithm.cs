
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuHatsune10thTSP
{
    static class GeneticAlgorithm
    {
        public static List<int[]> Initialize(int populationNumber, int cityNumber, Random rand)
        {
            var data = new List<int[]>(populationNumber);
            for (int i = 0; i < populationNumber; i++)
            {

                var temp = new int[cityNumber];
                for (int j = 0; j < cityNumber; j++)
                {
                    temp[j] = j + 1;
                }

                FisherYatesshuffle(temp, rand);
                data.Add(temp);
            }
            return data;
        }

        public static Pair<int[], int[]> Crossover(int[] parent1, int[] parent2, double CrossoverRate, Random rand)
        {
            int crossoverNum = (int)(CrossoverRate * parent1.Length);
            var crossoverPoints = new List<int>(parent1.Length);
            for (int i = 0; i < parent1.Length; i++)
            {
                crossoverPoints.Add(i + 1);
            }
            FisherYatesshuffle(crossoverPoints, rand);
            crossoverPoints.RemoveRange(crossoverNum, crossoverPoints.Count - crossoverNum);
            var parent1ExchangeIndex = new List<int>();
            var parent2ExchangeIndex = new List<int>();
            foreach (var item in crossoverPoints)
            {
                //ぐぇ…こ↑こ↓ O(n^2)でやめたくなりますよ～
                parent1ExchangeIndex.Add(Array.IndexOf(parent1, item));
                parent2ExchangeIndex.Add(Array.IndexOf(parent2, item));
            }
            parent1ExchangeIndex.Sort();
            parent2ExchangeIndex.Sort();
            var child1 = new int[parent1.Length];
            var child2 = new int[parent1.Length];
            parent1.CopyTo(child1, 0);
            parent2.CopyTo(child2, 0);
            for (int i = 0; i < parent1ExchangeIndex.Count; i++)
            {
                //swapする
                var temp = child1[parent1ExchangeIndex[i]];
                child1[parent1ExchangeIndex[i]] = child2[parent2ExchangeIndex[i]];
                child2[parent2ExchangeIndex[i]] = temp;
            }
            return new Pair<int[], int[]>(child1, child2);
        }
        private static void Mutation(int[] individual, double randNum, Random rand)
        {
            ///前から線形にガチャを引いていき、当たったら、ガチャを引いて適当な位置とスワップする。

            for (int i = 0; i < individual.Length; i++)
            {
                if (rand.NextDouble() < randNum)
                {
                    //入れ替えを実行
                    //Swap
                    var temp = individual[i];
                    var right = rand.Next(1, individual.Length - 1);//入れ替え先もガチャを引く
                    individual[i] = individual[right];
                    individual[right] = temp;
                }
            }

        }
        private static void Mutation2(int[] individual, double randNum, Random rand)
        {
            ///前から線形にガチャを引いていき、当たったら、隣(i+1と入れ替える)

            for (int i = 0; i < individual.Length; i++)
            {
                if (rand.NextDouble() < randNum)
                {
                    //入れ替えを実行
                    //Swap
                    var temp = individual[i];
                    if (i + 1 < individual.Length)
                    {
                        individual[i] = individual[i + 1];
                        individual[i + 1] = temp;
                    }
                    else
                    {
                        individual[i] = individual[0];
                        individual[0] = temp;
                    }
                }
            }

        }
        public static List<int[]> RunningGA(List<int[]> population, List<Pair<int, double>> fitness, Random[] rand, double crossoverRate, double mutationRate, int cityNumber, CalcFitness calc)
        {
            //make parents pair pool
            var parentPoolNumber = 250;
            var parentsPool = MakeParentsPool(parentPoolNumber, cityNumber, rand[0]);//rand[0] is temp parameter
            var eliteNumber = 20;
            var populationNumber = 200;
            //make new children
            while (parentsPool.Count != 0)
            {
                Crossover(parentsPool.Dequeue(), population, crossoverRate, rand[0]);
            }
            //calc fitness
            fitness.Clear();
            foreach (var item in population)
            {
                var hoge = fitness.Count;
                var temp = calc.Calc(item);
                fitness.Add(new Pair<int, double>(hoge, temp));
            }
            fitness.Sort((a, b) => b.Second.CompareTo(a.Second));
            //数件をelite保存して、下をルーレット選択する
            var nextGenerationList = new List<int>();
            for (int i = 0; i < eliteNumber; i++)
            {
                nextGenerationList.Add(fitness[i].First);
            }
            //ルーレット選択
            nextGenerationList.AddRange(RouletteWheelSelection(fitness, rand[0], eliteNumber, populationNumber));
            var newPopulation = new List<int[]>();
            for (int i = 0; i < nextGenerationList.Count; i++)
            {
                var temp = new int[cityNumber];
                population[nextGenerationList[i]].CopyTo(temp, 0);
                newPopulation.Add(temp);
            }
            //debug code
            fitness.Clear();
            foreach (var item in newPopulation)
            {
                var hoge = fitness.Count;
                var temp = calc.Calc(item);
                fitness.Add(new Pair<int, double>(hoge, temp));
            }
            fitness.Sort((a, b) => b.Second.CompareTo(a.Second));
            return newPopulation;

        }

        private static void Crossover(Pair<int, int> pair, List<int[]> population, double crossoverRate, Random random)
        {
            int crossoverNum = (int)(crossoverRate * population[pair.First].Length);
            var crossoverPoints = new List<int>(population[pair.First].Length);
            for (int i = 0; i < population[pair.First].Length; i++)
            {
                crossoverPoints.Add(i + 1);
            }
            FisherYatesshuffle(crossoverPoints, random);
            crossoverPoints.RemoveRange(crossoverNum, crossoverPoints.Count - crossoverNum);
            var parent1ExchangeIndex = new List<int>();
            var parent2ExchangeIndex = new List<int>();
            foreach (var item in crossoverPoints)
            {
                //ぐぇ…こ↑こ↓ O(n^2)でやめたくなりますよ～
                parent1ExchangeIndex.Add(Array.IndexOf(population[pair.First], item));
                parent2ExchangeIndex.Add(Array.IndexOf(population[pair.Second], item));
            }
            parent1ExchangeIndex.Sort();
            parent2ExchangeIndex.Sort();
            var child1 = new int[population[pair.First].Length];
            var child2 = new int[population[pair.First].Length];
            population[pair.First].CopyTo(child1, 0);
            population[pair.Second].CopyTo(child2, 0);
            //Mutation
            Mutation2(child1, 0.005, random);
            Mutation2(child2, 0.005, random);
            population.Add(child1);
            population.Add(child2);
        }

        private static Queue<Pair<int, int>> MakeParentsPool(int parentsPairNumber, int cityNumber, Random rand)
        {
            var pool = new Queue<Pair<int, int>>();
            int parent1, parent2;
            for (int i = 0; i < parentsPairNumber; i++)
            {
                parent1 = rand.Next(cityNumber);
                do
                {
                    parent2 = rand.Next(cityNumber);
                } while (parent1 == parent2);
                // choose 2 integer  parent1 != parent2 && 0<= parent1,parent2<=(populationNumber-1)
                pool.Enqueue(new Pair<int, int>(parent1, parent2));
            }
            return pool;
        }
        private static List<int> RouletteWheelSelection(List<Pair<int, double>> fitness, Random rand, int eliteNumber, int populationNumber)
        {
            var ans = new List<int>();
            var cumlativeSum = new List<double>();
            for (int i = 0; i < populationNumber; i++)
            {
                if (i < 20)
                    cumlativeSum.Add(0);
                else if (cumlativeSum.Count != 0)
                    cumlativeSum.Add(cumlativeSum.Last() + fitness[i].Second);
                else
                    cumlativeSum.Add(fitness[i].Second);
            }
            while (ans.Count < (populationNumber - eliteNumber))
            {
                var randNum = rand.NextDouble();
                for (int i = 0; i < cumlativeSum.Count; i++)
                {
                    if (randNum < ((cumlativeSum[i]) / cumlativeSum.Last()))
                    {
                        ans.Add(i);
                        break;
                    }
                }
            }
            return ans;
        }
        public static List<int> MakIndividual(int number)
        {
            var list = new List<int>();
            for (int i = 0; i < number; i++)
            {
                list.Add(i + 1);
            }
            return list;
        }

        public static void FisherYatesshuffle<Type>(IList<Type> a, Random rand)
        {
            int length = a.Count;
            for (int i = length - 1; i > 0; i--)
            {
                int j = rand.Next(i);
                var temp = a[i];
                a[i] = a[j];
                a[j] = temp;
            }
        }

    }
}