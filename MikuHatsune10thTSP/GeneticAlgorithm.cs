
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
                var temp = parentsPool.Dequeue();
                var cutPoint = (int)(194 * (0.3 + 0.4 * rand[0].NextDouble()));
                var children = Crossover((population[temp.Item1], population[temp.Item2]), cutPoint, rand[0]);
                Mutation(children.Item1, 0.05, rand[0]);
                Mutation(children.Item2, 0.05, rand[0]);
                population.Add(children.Item1);
                population.Add(children.Item2);
            }
            //calc fitness
            fitness.Clear();
            for (int i = 0; i < population.Count; i++)
            {
                fitness.Add(new Pair<int, double>(i, calc.Calc(population[i])));
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
            fitness.Sort((a, b) => a.First.CompareTo(b.First));
            nextGenerationList.Sort();
            for (int i = 0; i < nextGenerationList.Count; i++)
            {
                population[i] = (int[])population[nextGenerationList[i]].Clone();
            }
            fitness.RemoveRange(nextGenerationList.Count, population.Count - nextGenerationList.Count);
            population.RemoveRange(nextGenerationList.Count, population.Count - nextGenerationList.Count);
            fitness.Sort((a, b) => b.Second.CompareTo(a.Second));
            return population;

        }


        private static (int[], int[]) Crossover((int[], int[]) parents, int cutPoint, Random random)
        {
            //順序交差
            var children = (new int[parents.Item1.Length], new int[parents.Item1.Length]);
            var child1Front = new int[cutPoint];
            var child2Front = new int[cutPoint];
            Buffer.BlockCopy(parents.Item1, 0, child1Front, 0, cutPoint * 4);
            Buffer.BlockCopy(parents.Item2, 0, child2Front, 0, cutPoint * 4);

            var child1Sort = new int[child1Front.Length];
            var child2Sort = new int[child1Sort.Length];
            child1Front.CopyTo(child1Sort, 0);
            child2Front.CopyTo(child2Sort, 0);
            Array.Sort(child1Sort);
            Array.Sort(child2Sort);
            var child1Rear = new List<int>();
            var child2Rear = new List<int>();
            foreach (var item in parents.Item1)
            {
                if (Array.BinarySearch(child2Sort, item) < 0)
                {
                    child2Rear.Add(item);
                }
            }
            foreach (var item in parents.Item2)
            {
                if (Array.BinarySearch(child1Sort, item) < 0)
                {
                    child1Rear.Add(item);
                }
            }
            child1Front.CopyTo(children.Item1, 0);
            child2Front.CopyTo(children.Item2, 0);
            child1Rear.CopyTo(children.Item1, child1Front.Length);
            child2Rear.CopyTo(children.Item2, child2Front.Length);
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
                BREAK:
                var randNum = rand.NextDouble();
                for (int i = 0; i < cumlativeSum.Count; i++)
                {
                    if (randNum < ((cumlativeSum[i]) / cumlativeSum.Last()))
                    {
                        foreach (var item in ans)
                        {
                            if (fitness[i].First == item) goto BREAK;
                        }
                        ans.Add(fitness[i].First);
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