
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
        public static int[][] Initialize(int number, Random rand)
        {
            int[][] data = new int[20][];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                data[i] = new int[number];
                for (int j = 0; j < number; j++)
                {
                    data[i][j] = j + 1;
                }

                FisherYatesshuffle(data[i], rand);
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
        public static void MakeChildren(int[][] population, Pair<int, double>[] fitness, Random[] rand, double crossoverRate, double mutationRate)
        {
            List<int[]> parentsPool = new List<int[]>();
            var parents = ChooseParents(fitness, rand[0]);
            var parent1 = new int[population[parents.First].Length];
            population[parents.First].CopyTo(parent1, 0);
            var parent2 = new int[population[parents.Second].Length];
            population[parents.Second].CopyTo(parent2, 0);
            var emptyQueue = new Queue<int>();
            for (int i = 0; i < 20; i++)
            {
                if (fitness[0].First != i && fitness[1].First != i)
                    emptyQueue.Enqueue(i);
            }
            var syncObject = new Object();
#if false

            for (int i = 1; i < population.GetLength(0) / 2; i++)
            {
                int child1, child2;
                lock (syncObject)
                {
                    child1 = emptyQueue.Dequeue();
                    child2 = emptyQueue.Dequeue();
                }
                var children = Crossover(parent1, parent2, crossoverRate, rand[i]);

                Mutation2(children.First, mutationRate, rand[i]);
                Mutation2(children.Second, mutationRate, rand[i]);

                for (int j = 0; j < population[0].Length; j++)
                {
                    population[child1][j] = children.First[j];
                    population[child2][j] = children.Second[j];
                }
            }
#else
            Parallel.For(1, population.GetLength(0) / 2, i =>
            {
                int child1, child2;
                lock (syncObject)
                {
                    child1 = emptyQueue.Dequeue();
                    child2 = emptyQueue.Dequeue();
                }
                var children = Crossover(parent1, parent2, crossoverRate, rand[i]);

                Mutation2(children.First, mutationRate, rand[i]);
                Mutation2(children.Second, mutationRate, rand[i]);

                for (int j = 0; j < population[0].Length; j++)
                {
                    population[child1][j] = children.First[j];
                    population[child2][j] = children.Second[j];
                }
            });
#endif
        }
        private static Pair<int, int> ChooseParents(Pair<int, double>[] fitness, Random rand)
        {
            var cumlativeSum = new List<double>();
            int parent1 = 20, parent2 = 20;
            foreach (var item in fitness)
            {
                if (cumlativeSum.Count != 0)
                    cumlativeSum.Add(cumlativeSum.Last() + item.Second);
                else
                    cumlativeSum.Add(item.Second);
            }

            var randNum = rand.NextDouble();
            for (int i = 0; i < fitness.Length; i++)
            {
                if (randNum < ((cumlativeSum[i]) / cumlativeSum.Last()))
                {
                    parent1 = i;
                    break;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                randNum = rand.NextDouble();
                for (int j = 0; j < fitness.Length; j++)
                {
                    if (randNum < ((cumlativeSum[j]) / cumlativeSum.Last()))
                    {
                        parent2 = j;
                        break;
                    }
                }
                if (parent1 != parent2) break;
            }
            return new Pair<int, int>(parent1, parent2);
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