using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CsvEngine.Enums;
using System.Text;

namespace CsvEngine
{
    public class Csv
    {
        #region Properties
        public List<KeyValuePair<Error, string>> Errors { get; private set; }
        private Type MappingInstance { get; set; }
        #endregion

        #region constructor
        public Csv()
        {
            this.Errors = new List<KeyValuePair<Error, string>>();
        }
        #endregion

        /// <summary>
        /// Gerar csv baseado em uma lista de objeto fornecido.
        /// Colunas das propriedades csv seguem a mesma ordem de criacao das propriedades do objeto que sera mapeado
        /// </summary>
        /// <typeparam name="T">Qualquer objeto</typeparam>
        /// <param name="lista">lista de qualuqer objeto fornecido para gerar estrutura csv</param>
        /// <returns></returns>
        public string BuildCsv<T>(List<T> lista)
        {
            string csv = null;

            if (lista.GetType().Name != typeof(List<>).Name)
            {
                this.AddErrorMessage(Error.CRITICAL, $"Objetos fornecidos não está contido em uma {typeof(List<>)} para geração de csv!");
                return csv;
            }
            else
            {
                string lineProperties = this.CreatePropertiesLine(lista[0].GetType().GetProperties());
                csv = csv + lineProperties;

                for (int a = 0; a < lista.Count(); a++)
                {
                    string comma = ";";
                    string newLine = Environment.NewLine;

                    PropertyInfo[] properties = lista[a].GetType().GetProperties();

                    for (int i = 0; i < properties.Count(); i++)
                    {
                        if (properties[i].CanRead && properties[i].CanWrite)
                        {
                            object value = properties[i].GetValue(lista[a]);

                            if (i + 1 == properties.Count())
                                comma = string.Empty;

                            csv = csv + $"{value}{comma}";
                        }
                    }

                    if (a + 1 == lista.Count())
                        newLine = string.Empty;

                    csv = csv + $"{newLine}";
                }

                return csv;
            }
        }

        /// <summary>
        /// Mapeia string csv para class Dto fornecida
        /// </summary>
        /// <typeparam name="T">Qualque class Dto para realizar mapeamento</typeparam>
        /// <param name="csvString">string Csv que deve ser utilizada no mapeamento</param>
        /// <returns>Lista da class dto fornecida</returns>
        public List<T> CsvToObject<T>(string csvString, bool CsvAnalysis = true) where T : new()
        {
            PropertyInfo[] properties = this.GetDtoProperties<T>();
            var retorno = new List<T>();
            this.MappingInstance = typeof(T);

            int numberProperties = this.TotalCsvProperties(csvString);

            if (CsvAnalysis)
                this.CsvAnalysis(numberProperties, csvString);

            if (!this.Errors.Any(x => x.Key == Error.CRITICAL))
            {
                try
                {
                    string[] lines = this.GetLines(csvString, true);
                    string[] csvProperties = this.GetCsvProperties(csvString);

                    for (int i = 0; i < lines.Count(); i++)
                    {
                        string[] values = lines[i].Split(';');

                        T obj = new T();

                        for (int v = 0; v < values.Length; v++)
                        {
                            PropertyInfo property = this.GetPropertyInfo(properties, csvProperties[v]);
                            object tipoValorPropriedade = this.TiparValor(property.PropertyType, values[v]);
                            property.SetValue(obj, tipoValorPropriedade);
                        }

                        retorno.Add(obj);
                    }
                }
                catch (Exception)
                {
                    this.AddErrorMessage(Error.CRITICAL, $"Ocorreu um erro inesperado");
                }
            }

            return retorno;
        }

        #region Metodos Auxiliares

        /// <summary>
        /// Pega todas as propriedades da class dto existentes
        /// </summary>
        /// <typeparam name="T">Qualquer class dto</typeparam>
        /// <returns></returns>
        private PropertyInfo[] GetDtoProperties<T>() where T : new()
        {
            PropertyInfo[] propriedades = new T().GetType()?.GetProperties();
            return propriedades;
        }

        /// <summary>
        /// Retorna total de propriedades encontradas no csv
        /// </summary>
        /// <param name="linhas">string csv completa</param>
        /// <returns></returns>
        private int TotalCsvProperties(string linhas)
        {
            string[] _linhas = linhas.Split(Environment.NewLine);
            var columns = _linhas[0].Split(';');
            return columns.Count();
        }

        /// <summary>
        /// Retorna das as propriedades do csv encontradas
        /// </summary>
        /// <param name="linhas">string csv completa</param>
        /// <returns></returns>
        private string[] GetCsvProperties(string csv)
        {
            string[] csvLines = csv.Split(Environment.NewLine);
            var columns = csvLines[0].Split(';');
            return columns;
        }

        /// <summary>
        /// Gera linhas do csv informado
        /// </summary>
        /// <param name="csvString">String completa do Csv</param>
        /// <param name="linesToJump">A partir de qual linha deve retornar os dados</param>
        /// <returns></returns>
        private string[] GetLines(string csvString, bool skipProperties)
        {
            var _lines = csvString.Split(Environment.NewLine);

            if (skipProperties)
                _lines = _lines.Skip(1).ToArray();
            else _lines = _lines.ToArray();

            return _lines;
        }

        /// <summary>
        /// Pega tipo da propriedade passado da class Dto e retorna valor tipado
        /// </summary>
        /// <param name="columType"> Tipo da coluna da propriedade passada da class Dto</param>
        /// <param name="value">Valor fornecido do csv que sera tipado </param>
        /// <returns></returns>
        private object TiparValor(Type columType, object value)
        {
            try
            {
                object response = Convert.ChangeType(value, columType);
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Localiza qual propriedade da class Dto fornecida e igual da propriedade do csv fornecida
        /// </summary>
        /// <param name="nomePropriedades">Propriedades da class Dto fornecida</param>
        /// <param name="column">Propriedade do Csv para ser localizado na class Dto fornecida</param>
        /// <returns></returns>
        private PropertyInfo GetPropertyInfo(PropertyInfo[] nomePropriedades, string column)
        {
            PropertyInfo index = default(PropertyInfo);
            string nomePropriedade = default(string);
            bool located = false;

            for (int i = 0; i < nomePropriedades.Length; i++)
            {
                nomePropriedade = nomePropriedades[i].Name;

                if (nomePropriedade == column)
                {
                    index = nomePropriedades[i];
                    located = true;
                    break;
                }
                else if (nomePropriedade.ToLower() == column.ToLower())
                {
                    index = nomePropriedades[i];
                    located = true;

                    this.AddErrorMessage(Error.WARNING, $"Propriedade {column} está diferente da propriedade {nomePropriedade} da instancia informada {this.MappingInstance}");
                    break;
                }
            }

            if (!located)
            {
                this.AddErrorMessage(Error.CRITICAL, $"Propriedade {column} não localizada na class fornecida");
            }

            return index;
        }

        private void AddErrorMessage(Error error, string description)
        {
            this.Errors.Add(new KeyValuePair<Error, string>(error, $"{description}"));
        }

        public void CsvAnalysis(int sizeToSatisfyColumn, string csv)
        {
            string[] _lines = GetLines(csv, true);

            for (int li = 0; li < _lines.Count(); li++)
            {
                string[] _colums = _lines[li].Split(';');

                if (string.IsNullOrEmpty(_lines[li]))
                    this.AddErrorMessage(Error.NOTICE, $"Linha {li} está vazia ou nula");

                if (_colums.Count() <= 0)
                    this.AddErrorMessage(Error.NOTICE, $"Linha {li} não tem colunas");


                if (_colums.Count() > sizeToSatisfyColumn)
                {
                    this.AddErrorMessage(Error.CRITICAL, $"Foi identificado que a linha {li} tem mais colunas({_colums.Count()}) do que a quantidade de propriedades({sizeToSatisfyColumn}) do arquivo csv");
                }
                else if (_colums.Count() < sizeToSatisfyColumn)
                {
                    this.AddErrorMessage(Error.CRITICAL, $"Foi identificado que a linha {li} tem menos colunas({_colums.Count()}) do que a quantidade de propriedades({sizeToSatisfyColumn}) do arquivo csv");
                }
            }
        }

        /// <summary>
        /// Cria uma linha com as propriedades das colunas fornecidas e adiciona uma quebra de linha no final
        /// </summary>
        /// <param name="properties">Propriedades que sera do csv</param>
        /// <returns></returns>
        private string CreatePropertiesLine(PropertyInfo[] properties)
        {
            var prop = new StringBuilder(string.Empty);

            for (int i = 0; i < properties.Count(); i++)
            {
                if (prop.ToString() == string.Empty)
                    prop.Append($"{properties[i].Name}");
                else prop.Append($";{properties[i].Name}");
            }

            prop.Append(Environment.NewLine);

            return prop.ToString();
        }

        #endregion
    }
}
