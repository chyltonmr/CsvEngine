using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CsvFile.Test;
using CsvFile.Enums;

namespace CsvFile
{
    public class Csv
    {
        #region Properties
        public List<KeyValuePair<Error, string>> Errors { get; private set; }
        private List<string> DifferentProperties { get; set; }
        private Type MappingInstance { get; set; }
        #endregion

        #region constructor
        public Csv()
        {
            this.Errors = new List<KeyValuePair<Error, string>>();
            this.DifferentProperties = new List<string>();
        }
        #endregion

        //TODO: Criar comentario na class e tamb verificar se e possivel gerar log de erros tamb no metodo
        public string BuildCsv<T>(List<T> lista)
        {
            string csv = null;

            if (lista.GetType().Name != typeof(List<>).Name)
            {
                this.AddErrorMessage(Error.CRITICAL, $"Ocorreu um erro inesperado");
                return csv;
            }
            else
            {
                for (int a = 0; a < lista.Count(); a++)
                {
                    string comma = ";";
                    string newLine = Environment.NewLine;

                    PropertyInfo[] properties = lista[a].GetType().GetProperties();

                    for (int i = 0; i < properties.Count(); i++)
                    {
                        if (properties[i].CanRead && properties[i].CanWrite)
                        {
                            var value = properties[i].GetValue(lista[a]);

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
        /// <param name="linhas">string Csv que deve ser utilizada no mapeamento</param>
        /// <returns>Lista da class dto fornecida</returns>
        public List<T> CsvToObject<T>(string linhas, bool CsvAnalysis = true) where T : new()
        {
            PropertyInfo[] properties = this.GetDtoProperties<T>();
            var retorno = new List<T>();
            this.MappingInstance = typeof(T);

            (string[] columns, int numberColumns) = this.GetColumns(linhas);

            if (CsvAnalysis)
                this.CsvAnalysis(numberColumns, linhas);

            if (!this.Errors.Any(x => x.Key == Error.CRITICAL))
            {
                try
                {
                    string[] lines = this.GetLines(linhas);

                    for (int i = 0; i < lines.Count(); i++)
                    {
                        string[] values = lines[i].Split(';');

                        T obj = new T();

                        for (int v = 0; v < values.Length; v++)
                        {
                            PropertyInfo property = this.GetPropertyInfo(properties, columns[v]);
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
        /// Retorna todas as colunas que representa as propriedades da class Dto
        /// </summary>
        /// <param name="linhas">string csv completa</param>
        /// <returns></returns>
        private (string[], int) GetColumns(string linhas)
        {
            string[] _linhas = linhas.Split(Environment.NewLine);
            var columns = _linhas[0].Split(';');
            return (columns, columns.Count());
        }

        /// <summary>
        /// Retorna somente as linhas dos valores, retirando a primeira linha das propriedades
        /// </summary>
        /// <param name="lines">String completa do Csv</param>
        /// <returns></returns>
        private string[] GetLines(string lines)
        {
            var _lines = lines.Split(Environment.NewLine);
            _lines = _lines.Skip(1).ToArray();
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
                    string prop = this.DifferentProperties.SingleOrDefault(x => x == column);
                    if (string.IsNullOrEmpty(prop))
                    {
                        this.DifferentProperties.Add(column);
                        this.AddErrorMessage(Error.WARNING, $"Propriedade {column} está diferente da propriedade {nomePropriedade} da instancia informada {this.MappingInstance}");
                    }
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

        private void CsvAnalysis(int sizeToSatisfyColumn, string csv)
        {
            string[] _lines = csv.Split(Environment.NewLine);

            _lines = _lines.Skip(1).ToArray();

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

        #endregion
    }
}
