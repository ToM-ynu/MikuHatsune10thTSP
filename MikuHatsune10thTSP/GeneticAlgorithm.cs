
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
        public static void Initialize(int[][][] population, int cityNumber, int maxPopulationNumber, Random rand)
        {
            var hoge = new int[maxPopulationNumber][];
            for (int i = 0; i < hoge.GetLength(0); i++)
            {
                var temp = new int[cityNumber];

                for (int j = 0; j < cityNumber; j++)
                {
                    temp[j] = j + 1;
                }
                FisherYatesshuffle(temp, rand);
                hoge[i] = temp;
            }
            population[0] = hoge;
            population[1] = hoge;
        }
        private static void Invart(int[] individual, (int, int) section)
        {
            int i = section.Item1, j = section.Item2;
            while (true)
            {
                if (j <= i) break;
                var temp = individual[i];
                individual[i] = individual[j];
                individual[j] = temp;
                i++;
                j--;
            }
        }
        private static void Mutation(int[] individual, double randNum, Random random)
        {
            for (int i = 0; i < individual.Length; i++)
            {
                if (random.NextDouble() < randNum)
                {
                    //逆順(ある区間を繋ぎ変えて、それ以外を何とかする方法)
                    var section = new int[2];
                    section[0] = random.Next(individual.Length);
                    section[1] = random.Next(individual.Length);
                    Array.Sort(section);
                    Invart(individual, (section[0], section[1]));
                }
            }
        }
        public static void RunningGA(int[][][] population, (int, double)[] fitness, Random[] rand, double crossoverRate, double mutationRate, int cityNumber, CalcFitness calc)
        {
            //make parents pair pool

            const int eliteNumber = 20;
            const int populationNumber = 200;
            const int parentPoolNumber = 250;
            var parentsPool = MakeParentsPool(parentPoolNumber, cityNumber, rand[0]);//rand[0] is temp parameter

            //make new children
            for (int i = 0; parentsPool.Count > 0; i += 2)
            {
                var temp = parentsPool.Dequeue();
                var cutPoint = (int)(38 * (0.7 + 0.1 * rand[0].NextDouble()));
                var children = Crossover((population[0][temp.Item1], population[0][temp.Item2]), cutPoint, rand[0]);
                Mutation(children.Item1, 0.01, rand[0]);//1%
                Mutation(children.Item2, 0.01, rand[0]);//1%
                population[1][populationNumber + i] = (children.Item1);
                population[1][populationNumber + i + 1] = (children.Item2);
            }
            //今の世代を0->1へ移動させる
            for (int i = 0; i < populationNumber; i++)
            {
                population[1][i] = (int[])population[0][i].Clone();
            }

            for (int i = 0; i < population[0].GetLength(0); i++)
            {
                fitness[i] = (i, calc.Calc(population[1][i]));
            }
            //評価値が良い順番に並べる
            Array.Sort(fitness, (a, b) => b.Item2.CompareTo(a.Item2));
            //数件をelite保存して、下をルーレット選択する
            var nextGenerationList = new List<int>();
            for (int i = 0; i < eliteNumber; i++)
            {
                nextGenerationList.Add(fitness[i].Item1);
            }
            //ルーレット選択
            nextGenerationList.AddRange(RouletteWheelSelection(fitness, rand[0], eliteNumber, populationNumber));
            //↑fitness順に並んでいる
            nextGenerationList.Sort();
            for (int i = 0; i < nextGenerationList.Count; i++)
            {
                population[0][i] = (int[])population[1][nextGenerationList[i]].Clone();
            }

        }

        //それなりに遅いが、GCは旧Crossoverの1/10ぐらい
        //コードも単純なので見やすい？
        private static (int[], int[]) Crossover((int[], int[]) parents, int cutPoint, Random random)
        {
            //順序交差
            var children = (new int[parents.Item1.Length], new int[parents.Item2.Length]);
            //変更のないところをそのままコピーする
            for (int i = 0; i < cutPoint; i++)
            {
                children.Item1[i] = parents.Item1[i];
                children.Item2[i] = parents.Item2[i];
            }
            int left = 0;
            for (int i = cutPoint; i < parents.Item1.Length; i++)
            {
                for (int j = left; j < parents.Item2.Length; j++)
                {
                    for (int k = cutPoint; k < parents.Item1.Length; k++)
                    {
                        if (parents.Item2[j] == parents.Item1[k])
                        {
                            //あった
                            children.Item1[i] = parents.Item2[j];
                            left = j + 1;
                            goto LoopOut;
                        }
                    }
                }
                LoopOut:;
            }
            left = 0;
            for (int i = cutPoint; i < parents.Item2.Length; i++)
            {
                for (int j = left; j < parents.Item1.Length; j++)
                {
                    for (int k = cutPoint; k < parents.Item2.Length; k++)
                    {
                        if (parents.Item1[j] == parents.Item2[k])
                        {
                            //あった
                            children.Item2[i] = parents.Item1[j];
                            left = j + 1;
                            goto LoopOut;
                        }
                    }
                }
                LoopOut:;
            }
            return children;
        }

        private static Queue<(int, int)> MakeParentsPool(int parentsPairNumber, int cityNumber, Random rand)
        {
            var pool = new Queue<(int, int)>();
            int parent1, parent2;
            for (int i = 0; i < parentsPairNumber; i++)
            {
                parent1 = rand.Next(cityNumber);
                do
                {
                    parent2 = rand.Next(cityNumber);
                } while (parent1 == parent2);
                // choose 2 integer  parent1 != parent2 && 0<= parent1,parent2<=(populationNumber-1)
                pool.Enqueue((parent1, parent2));
            }
            return pool;
        }
        private static List<int> RouletteWheelSelection((int, double)[] fitness, Random rand, int eliteNumber, int populationNumber)
        {
            var ans = new List<int>();
            var cumlativeSum = new List<double>();
            for (int i = 0; i < fitness.Length; i++)
            {

                if (cumlativeSum.Count != 0)
                    cumlativeSum.Add(cumlativeSum.Last() + fitness[i].Item2);
                else
                    cumlativeSum.Add(fitness[i].Item2);
            }
            double randNum;
            while (ans.Count < (populationNumber - eliteNumber))
            {
                randNum = rand.NextDouble();
                for (int i = 0; i < cumlativeSum.Count; i++)
                {
                    if (randNum < (cumlativeSum[i] / cumlativeSum.Last()))
                    {
                        ans.Add(fitness[i].Item1);
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