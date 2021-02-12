using CsvFile;
using CsvFile.Enums;
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

            string linhas = "Uint;DateTime;Nome;Idade" + Environment.NewLine + "1;12/11/2021;chylton" + Environment.NewLine + "1;12/11/2021;Lidiane;33;55;990";

            var csv = new Csv();
            List<Chylton> response = csv.CsvMap<Chylton>(linhas);

            if (csv.Errors.Any())
            {
                var erros = "Existe erros";
            }
        }
    }
}
