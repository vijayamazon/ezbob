using System.Collections.Generic;

namespace EzBobPersistence {
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Common.Logging;

    public static class Extensions {

        private static readonly ILog DefaultLog = LogManager.GetLogger("ModelsMapper");

        /// <summary>
        /// Maps to numerical collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static IEnumerable<T> MapToNumericalCollection<T>(this SqlCommand command) where T : struct {
            using (var sqlCommand = command) {
                return CreatePrimitivesCollection<T>(sqlCommand.ExecuteReader());
            }
        }

        /// <summary>
        /// Maps to string collection.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static IEnumerable<string> MapToStringCollection(this SqlCommand command) {
            using (var sqlCommand = command) {
                return CreateStringsCollection(sqlCommand.ExecuteReader());
            }
        }

        /// <summary>
        /// Executes the commands and returns their statuses.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public static IEnumerable<bool> MapNonqueryExecuteToBool(this IEnumerable<SqlCommand> commands, ILog log = null) {
            foreach (var command in commands) {
                yield return command.MapNonqueryExecuteToBool(log);
            }
        }

        /// <summary>
        /// Executes the command and logs errors if there are.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public static bool MapNonqueryExecuteToBool(this SqlCommand command, ILog log = null) {
            try {
                if (log == null) {
                    log = DefaultLog;
                }

                int numberOfRowsAffected = command.ExecuteNonQuery();

                if (numberOfRowsAffected < 1) {
                    log.Error("got error while executing query");
                    return false;
                }
            } catch (SqlException ex) {
                log.Error(ex);
                throw;
            }

            return true;
        }

        /// <summary>
        /// Executes commands and maps them to models.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public static IEnumerable<T> MapToModels<T>(this IEnumerable<SqlCommand> commands) where T : class, new() {
            foreach (var collection in commands.Select(o => o.MapToModels<T>())) {
                foreach (var item in collection) {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Executes command and maps it to models.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static IEnumerable<T> MapToModels<T>(this SqlCommand command) where T : class, new() {
            using (var sqlCommand = command) {
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                while (dataReader.Read()) {
                    yield return CreateSingleModel<T>(dataReader);
                }
            }
        }

        /// <summary>
        /// Creates the specified model from data reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        private static T CreateSingleModel<T>(DbDataReader dataReader, ILog log = null) where T : class, new() {
            T instance = new T();

            if (log == null) {
                log = DefaultLog;
            }

            PropertyInfo[] properties = ObtainAllTypeProperties<T>();

            for (int i = 0; i < dataReader.FieldCount; ++i) {
                PropertyInfo property = FindWritablePropertyByName(properties, dataReader.GetName(i));
                if (property != null) {
                    Type propertyType = property.PropertyType;
                    object propertyValue = dataReader.GetValue(i);
                    if (propertyValue is DBNull) {
                        log.Debug("got DBNull");
                        continue;
                    }

                    Type nullableType = Nullable.GetUnderlyingType(propertyType);
                    if (nullableType != null) {
                        propertyType = nullableType;
                    }

                    if (Type.GetTypeCode(propertyType) == Type.GetTypeCode(dataReader.GetFieldType(i))) {
                        property.SetValue(instance, propertyValue);
                    } else {
                        log.Warn("reader type not equal to property type");
                        object result = null;
                        try {
                            result = Convert.ChangeType(propertyValue, Type.GetTypeCode(propertyType));
                        } catch (Exception ex) {
                            log.Error(string.Format("could not convert {0} to {1}", propertyValue.GetType()
                                .Name, propertyType.Name), ex);
                        }

                        if (result != null) {
                            property.SetValue(instance, result);
                        }
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates the primitives collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        private static IEnumerable<T> CreatePrimitivesCollection<T>(DbDataReader dataReader) where T : struct {
            while (dataReader.Read()) {
                yield return (T)dataReader.GetValue(0);
            }
        }

        /// <summary>
        /// Creates the strings collection.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        private static IEnumerable<string> CreateStringsCollection(DbDataReader dataReader) {
            while (dataReader.Read()) {
                yield return (string)dataReader.GetValue(0);
            }
        }

        /// <summary>
        /// Obtains all properties of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static PropertyInfo[] ObtainAllTypeProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Finds the writable property by name.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="dbPropertyName">Name of the property in DB.</param>
        /// <returns></returns>
        private static PropertyInfo FindWritablePropertyByName(PropertyInfo[] properties, string dbPropertyName) {
            return properties.Where(prop => prop.CanWrite)
                .FirstOrDefault(property => string.Equals(property.Name, dbPropertyName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
