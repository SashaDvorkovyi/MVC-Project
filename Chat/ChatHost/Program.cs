using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Chat;
using System.Data.Entity.Infrastructure;

namespace ChatHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(Chat.ServiceChat)))
            {
                
                host.Open();
                Console.WriteLine("Хост стартвал!");
                Console.ReadKey();
            }
        }


    }
}
