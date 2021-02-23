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

            string linhas = "uint;DateTime;Nome;Idade" + Environment.NewLine + "1;12/11/2021;chylton;24" + Environment.NewLine + "1;12/11/2021;Fernando;30";

            var csv = new Csv();
            var listaChylton = new List<Chylton>();
            var chy = new Chylton()
            {
                Idade = 34,
                Nome = "chylton"
            };
            listaChylton.Add(chy);

            var ad = csv.BuildCsv(listaChylton);

            List<Chylton> response = csv.CsvToObject<Chylton>(linhas);

            if (csv.Errors.Any())
            {
                var erros = "Existe erros";
            }
        }
    }
}
