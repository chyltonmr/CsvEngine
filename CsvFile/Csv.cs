using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using CsvFile.Test;

namespace CsvFile
{
    public class Csv
    {

        public void Teste()
        {
            string linhas = $"Nome;Idade{Environment.NewLine}chylton;34{Environment.NewLine}lidiane;33";

            var c = this.CsvMap<Chylton>(linhas);
        }

        /// <summary>
        /// Mapeia string csv para class Dto fornecida
        /// </summary>
        /// <typeparam name="T">Qualque class Dto para realizar mapeamento</typeparam>
        /// <param name="linhas">string Csv que deve ser utilizada no mapeamento</param>
        /// <returns>Lista da class dto fornecida</returns>
        public List<T> CsvMap<T>(string linhas) where T : new()
        {
            PropertyInfo[] properties = this.PegarPropriedadesDto<T>();
            var retorno = new List<T>();

            string[] columns = this.GetColumns(linhas);

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

            return retorno;
        }

        #region Metodos Auxiliares

        /// <summary>
        /// Pega todas as propriedades da class dto existentes
        /// </summary>
        /// <typeparam name="T">Qualquer class dto</typeparam>
        /// <returns></returns>
        private PropertyInfo[] PegarPropriedadesDto<T>() where T : new()
        {
            PropertyInfo[] propriedades = new T().GetType()?.GetProperties();
            return propriedades;
        }

        /// <summary>
        /// Retorna todas as colunas que representa as propriedades da class Dto
        /// </summary>
        /// <param name="linhas">string csv completa</param>
        /// <returns></returns>
        private string[] GetColumns(string linhas)
        {
            string[] _linhas = linhas.Split(Environment.NewLine);
            var columns = _linhas[0].Split(';');
            return columns;
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
            object response = Convert.ChangeType(value, columType);
            return response;
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

            for (int i = 0; i < nomePropriedades.Length; i++)
            {
                nomePropriedade = nomePropriedades[i].Name;

                if (nomePropriedade == column)
                {
                    index = nomePropriedades[i];
                    break;
                }
            }

            return index;
        }

        #endregion
    }
}
