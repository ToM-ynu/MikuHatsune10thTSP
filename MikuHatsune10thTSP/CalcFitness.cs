using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;
namespace MikuHatsune10thTSP
{

    public class CalcFitness
    {
        const double optimal = 6656;
        List<City> data;
        public CalcFitness(List<City> data)
        {
            this.data = data;
        }
        public double Calc(int[] item)
        {
            var ans = 0.0;
            for (int j = 0; j < item.Length - 1; j++)
            {
                ans += City2City(data[item[j] - 1], data[item[j + 1] - 1]);

            }
            ans += City2City(data[item[0] - 1], data[item[item.Length - 1] - 1]);
            return (optimal / ans);
        }

        private static double City2City(City left, City right)
        {

            return Sqrt(Pow(Abs(left.XAxis - right.XAxis), 2) + Pow(Abs(left.YAxis - right.YAxis), 2));
        }


    }
}
