using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(Server.ServiceLSB)))
            {
                host.Open();
                Console.WriteLine("OK");
                Console.ReadLine();
            }
        }
    }
}
