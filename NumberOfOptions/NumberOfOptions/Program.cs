using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberOfOptions
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] banknotes = { 200, 100, 50, 20, 10, 5, 2, 1 }; //number of pence in banknotes (£2 (200p), £1 (100p), 50p, 20p, 10p, 5p, 2p, 1p, in this task)
            int maxValue = 200; //value in pence for which we are looking for combinations (£2 in this task)
            int value = 0; //the value occupied in the cycles of the upper level
            int item = 0; //value for movement in an array
            Console.WriteLine(CountOptions(banknotes, maxValue, ref value, ref item));
            Console.ReadKey();
        }

        public static int CountOptions(int[]array, int maxValue, ref int value, ref int item)
        {
            var result = default(int);
            if (item == array.Length-2)
            {
                result += (maxValue - value) / array[item] + 1;
            }
            else
            {
                for(var i =0; i<= maxValue - value; i += array[item])
                {
                    value += i;
                    item++;

                    result += CountOptions(array, maxValue, ref value, ref item);

                    value -= i;
                    item--;
                }
            }
            return result;
        }
    }
}
