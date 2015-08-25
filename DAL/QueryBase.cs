using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobDAL
{
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Reflection;

    public abstract class QueryBase {

        private readonly string connectionString;

        protected QueryBase(String connectionString) {
            if (String.IsNullOrEmpty(connectionString)) {
                //TODO: log error
                return;
            }

            this.connectionString = connectionString;
        }

        public SqlConnection GetOpenedSqlConnection() {
            var sqlConnection = new SqlConnection(this.connectionString);
            try {
                sqlConnection.Open();
            } catch (Exception ex) {
                sqlConnection = null;
                //TODO: to log exception
            }

            return sqlConnection;
        }

        private void SubscribeForInfo(SqlConnection sqlConnection) {
//            sqlConnection.InfoMessage += (sender, args) => Log.Debug(
//                "Database information from '{0}'; message: '{1}'.\nErrors:\n\t{2}",
//                args.Source,
//                args.Message,
//                string.Join("\n\t", args.Errors)
//            );
        }

        protected T CreateModel<T>(DbDataReader dataReader) where T : class, new() {
            T instance = new T();

            PropertyInfo[] properties = obtainTypeProperties<T>();

            while (dataReader.Read()) {
                for (int i = 0; i < dataReader.FieldCount; ++i) {
                    PropertyInfo property = findProperty(properties, dataReader.GetName(i));
                    if (property != null) {
                        Type propertyType = property.PropertyType;
                        object propertyValue = dataReader.GetValue(i);
                        if (propertyValue is DBNull) {
                            //TODO: log debug db null
                            continue;
                        }

                        if (Type.GetTypeCode(propertyType) == Type.GetTypeCode(dataReader.GetFieldType(i))) {
                            property.SetValue(instance, propertyValue);
                        } else {
                            //TODO: log warning (different types)
                            object result = null;
                            try {
                                result = Convert.ChangeType(propertyValue, Type.GetTypeCode(propertyType));
                            } catch (Exception) {
                                //TODO: log error (could not convert)
                            }

                            if (result != null) {
                                property.SetValue(instance, result);
                            }
                        }
                    }
                }
            }

            return instance;
        }

        private PropertyInfo[] obtainTypeProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private PropertyInfo findProperty(PropertyInfo[] properties, string dbPropertyName) {
            foreach (var property in properties.Where(prop => prop.CanWrite)) {
                int indexOf = CultureInfo.InvariantCulture.CompareInfo.IndexOf(dbPropertyName, property.Name, CompareOptions.IgnoreCase);
                if (indexOf > -1) {
                    return property;
                }

                indexOf = CultureInfo.InvariantCulture.CompareInfo.IndexOf(property.Name, dbPropertyName, CompareOptions.IgnoreCase);
                if (indexOf > -1) {
                    return property;
                }
            }

            return null;
        }
    }
}
