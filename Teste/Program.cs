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

            string linhas = "Uint;DateTime;Nome;Idade" + Environment.NewLine + "1;12/11/2021;chylton;34" + Environment.NewLine + "1;12/11/2021;Lidiane;33";

            var csv = new Csv();
            List<Chylton> response = csv.CsvMap<Chylton>(linhas);

            if (csv.Errors.Properties.Any())
            {
                var tem = "Tem erro";
            }
        }
    }
}
