using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string line;
            // Read stdin in a loop
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine("\"it worked!\"," + line);
            }
        }
    }
}
