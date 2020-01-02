using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Changelogger
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramSettings.ReadSettings();
            Console.WriteLine("Read settings");
            LogRetriever logRetriever = new LogRetriever();
            logRetriever.ReadMasterList(args[0]);
            Console.WriteLine("Read masterlist");
            logRetriever.GenerateFullList(args[0]);
            Console.WriteLine("Generated full list");
            logRetriever.SaveFullList(args[0]);
            Console.WriteLine("Saved full list");
            logRetriever.HandleDiscrepancies(args[0]);
            Console.WriteLine("Complete");
            Console.ReadLine();
        }
    }
}
