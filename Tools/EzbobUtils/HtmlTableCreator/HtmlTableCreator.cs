namespace HtmlTableCreator
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    public class HtmlTableCreator
    {
        /// <summary>
        /// Creates html table from pairs
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="firstHeader"></param>
        /// <param name="secondHeader"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string CreateHtmlTableFromPairs<T1, T2>(string firstHeader, string secondHeader, IEnumerable<KeyValuePair<T1, T2>> content)
        {
            return CreateHtmlTable(CreateHtmlTableBody(content), firstHeader, secondHeader);
        }

        /// <summary>
        /// Creates html table with property reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfInstances"></param>
        /// <returns></returns>
        public static string CreateHtmlTableFromClass<T>(IEnumerable<T> listOfInstances)
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            var headers = new List<string>();
            foreach (PropertyInfo pi in propertyInfos)
            {
                headers.Add(pi.Name);
            }

            return CreateHtmlTableFromClassAndHeaders(listOfInstances, propertyInfos, headers);
        }

        /// <summary>
        /// Creates html table according to propertyToHeaderMap
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfInstances"></param>
        /// <param name="propertyToHeaderMap"></param>
        /// <returns></returns>
        public static string CreateHtmlTableFromClass<T>(IEnumerable<T> listOfInstances, Dictionary<string, string> propertyToHeaderMap)
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            var headers = new List<string>();
            foreach (PropertyInfo pi in propertyInfos)
            {
                headers.Add(propertyToHeaderMap[pi.Name]);
            }

            return CreateHtmlTableFromClassAndHeaders(listOfInstances, propertyInfos, headers);
        }

        private static string CreateHtmlTable(string body, params string[] headers)
        {
            var tableHeader = new StringBuilder();
            tableHeader.Append(Consts.TableHtmlStyle).Append(Consts.TableHeadOpenTag).Append(Consts.TrHeadHtmlStyle);
            foreach (string header in headers)
            {
                tableHeader.Append(Consts.ThHtmlStyle).Append(header).Append(Consts.TableHeaderCloseTag);
            }
            tableHeader.Append(Consts.TableRowCloseTag).Append(Consts.TableHeadCloseTag).Append(Consts.TableBodyOpenTag).Append(body).Append(Consts.TableBodyCloseTag).Append(Consts.TableEndTag);
            return tableHeader.ToString();
        }

        private static string CreateHtmlTableBody<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> content)
        {
            var tableBody = new StringBuilder();
            foreach (var pair in content)
            {
                tableBody.Append(Consts.TableRowOpenTag).Append(Consts.TdHtmlStyle)
                            .Append(pair.Key)
                            .Append(Consts.TableDataCloseTag).Append(Consts.TdHtmlStyle)
                            .Append(pair.Value)
                            .Append(Consts.TableDataCloseTag).Append(Consts.TableRowCloseTag);
            }

            return tableBody.ToString();
        }

        private static string CreateHtmlTableFromClassAndHeaders<T>(IEnumerable<T> listOfInstances, 
                                                                    PropertyInfo[] propertyInfos,
                                                                    List<string> headers)
        {
            var body = new StringBuilder();
            foreach (T instance in listOfInstances)
            {
                body.Append(Consts.TableRowOpenTag);
                foreach (PropertyInfo pi in propertyInfos)
                {
                    body.Append(Consts.TdHtmlStyle).Append(pi.GetValue(instance, null)).Append(Consts.TableDataCloseTag);
                }
                body.Append(Consts.TableRowCloseTag);
            }

            return CreateHtmlTable(body.ToString(), headers.ToArray());
        }
    }
}
