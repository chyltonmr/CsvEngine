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
        public List<KeyValuePair<Error, string>> Errors { get; private set; }

        public Csv()
        {
            this.Errors = new List<KeyValuePair<Error, string>>();
        }

        /// <summary>
        /// Mapeia string csv para class Dto fornecida
        /// </summary>
        /// <typeparam name="T">Qualque class Dto para realizar mapeamento</typeparam>
        /// <param name="linhas">string Csv que deve ser utilizada no mapeamento</param>
        /// <returns>Lista da class dto fornecida</returns>
        public List<T> CsvMap<T>(string linhas, bool CsvAnalysis = true) where T : new()
        {
            PropertyInfo[] properties = this.GetDtoProperties<T>();
            var retorno = new List<T>();

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
                    this.Errors.Add(new KeyValuePair<Error, string>(Error.CRITICAL, $"Ocorreu um erro inesperado"));
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
            }

            if (!located)
            {
                this.Errors.Add(new KeyValuePair<Error, string>(Error.CRITICAL, $"Propriedade {column} não localizada na class fornecida"));
            }

            return index;
        }

        private void CsvAnalysis(int sizeToSatisfyColumn, string csv)
        {
            string[] _lines = csv.Split(Environment.NewLine);

            _lines = _lines.Skip(1).ToArray();

            for (int li = 0; li < _lines.Count(); li++)
            {
                string[] _colums = _lines[li].Split(';');

                if (string.IsNullOrEmpty(_lines[li]))
                    this.Errors.Add(new KeyValuePair<Error, string>(Error.NOTICE, $"Linha {li} está vazia ou nula"));

                if (_colums.Count() <= 0)
                    this.Errors.Add(new KeyValuePair<Error, string>(Error.NOTICE, $"Linha {li} não tem colunas"));


                if (_colums.Count() > sizeToSatisfyColumn)
                {
                    this.Errors.Add(new KeyValuePair<Error, string>(Error.CRITICAL, $"Foi identificado que a linha {li} tem mais colunas({_colums.Count()}) do que a quantidade de propriedades({sizeToSatisfyColumn}) do arquivo csv"));
                }
                else if (_colums.Count() < sizeToSatisfyColumn)
                {
                    this.Errors.Add(new KeyValuePair<Error, string>(Error.CRITICAL, $"Foi identificado que a linha {li} tem menos colunas({_colums.Count()}) do que a quantidade de propriedades({sizeToSatisfyColumn}) do arquivo csv"));
                }
            }
        }

        #endregion
    }
}
