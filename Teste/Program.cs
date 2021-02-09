using CsvFile;
using CsvFile.Test;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Teste
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var csv = new Csv();
            csv.Teste();

            if (csv.Errors.Properties.Any())
            {
                var tem = "Tem erro";
            }

            List<Chylton> c = (List<Chylton>)csv.Data;
        }
    }
}
