namespace EzBobPersistence {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Transactions;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobPersistence.QueryGenerators;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

    /// <summary>
    /// Base class for all queries.
    /// Contains methods helping to work with database
    /// </summary>
    public abstract class QueryBase {

        private readonly string connectionString;
        private static readonly string comma = ",";
        private static readonly string strudel = "@";
        private static readonly string and = " AND ";


        protected QueryBase(String connectionString) {
            if (String.IsNullOrEmpty(connectionString)) {
                Log.Error("got empty connection string");
                return;
            }

            this.connectionString = connectionString;
        }


        public DatabaseConnectionWrapper GetOpenedSqlConnection2()
        {
            DatabaseConnectionWrapper wrapper;
            if (Transaction.Current != null) {
                wrapper = TransactionScopeConnections.GetConnection(new SqlDatabase(this.connectionString));
            } else {
                wrapper = new DatabaseConnectionWrapper(GetOpenedSqlConnection());
            }

            return wrapper;
        }

        /// <summary>
        /// Gets the opened SQL connection.
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetOpenedSqlConnection() {
            var sqlConnection = new SqlConnection(this.connectionString);

            try {
                sqlConnection.Open();
            } catch (Exception ex) {
                sqlConnection = null;
                Log.Error("could not open sql connection. connection string:" + this.connectionString, ex);
            }

            return sqlConnection;
        }


        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Converts provided object to <see cref="T:System.Data.DataTable" />.
        /// </summary>
        /// <typeparam name="T">class having public properties which names corresponds to db table name.
        /// Exceptions: From, To - converted to [From], [To]</typeparam>
        /// <param name="tableObj">The table object.</param>
        /// <param name="ignoreFields">The ignore fields - fields that we don't want to add to datatable.</param>
        /// <returns></returns>
        protected DataTable ConvertToDataTable<T>(T tableObj, string[] ignoreFields = null) where T : class {

            return ConvertToDataTable(Enumerable.Repeat(tableObj, 1), ignoreFields);
        }

        /// <summary>
        /// Converts to data table provided sequence.
        /// </summary>
        /// <typeparam name="T">class having public properties which names corresponds to db table name.</typeparam>
        /// <param name="tableObjects">The table objects sequence.</param>
        /// <param name="ignoreFields">The ignore fields - fields that we don't want to add to datatable.</param>
        /// <returns></returns>
        protected DataTable ConvertToDataTable<T>(IEnumerable<T> tableObjects, string[] ignoreFields = null) where T : class {
            DataTable dataTable = new DataTable();

            PropertyInfo[] properties = ObtainTypePublicProperties<T>();

            IEnumerable<string> ignore = ignoreFields ?? Enumerable.Empty<string>();

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (ignore.Contains(propertyInfo.Name)) {
                    continue;
                }

                if (propertyInfo.Name.Equals("from", StringComparison.InvariantCultureIgnoreCase) || propertyInfo.Name.Equals("to", StringComparison.InvariantCultureIgnoreCase))
                {
                    dataTable.Columns.Add(String.Format("[{0}]", propertyInfo.Name));
                    continue;
                }

                Type propertyType = propertyInfo.PropertyType;
                if (IsNullable(propertyType))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                }

                dataTable.Columns.Add(propertyInfo.Name, propertyType);
            }

            foreach (var tableObj in tableObjects)
            {
                DataRow row = dataTable.NewRow();

                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (ignore.Contains(propertyInfo.Name))
                    {
                        continue;
                    }

                    if (propertyInfo.Name.Equals("from", StringComparison.InvariantCultureIgnoreCase) || propertyInfo.Name.Equals("to", StringComparison.InvariantCultureIgnoreCase))
                    {
                        row[String.Format("[{0}]", propertyInfo.Name)] = propertyInfo.GetValue(tableObj);
                        continue;
                    }

                    row[propertyInfo.Name] = propertyInfo.GetValue(tableObj) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        /// Determines whether the specified property information is nullable.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        private static bool IsNullable(PropertyInfo propertyInfo) {
            return IsNullable(propertyInfo.PropertyType);
        }

        /// <summary>
        /// Determines whether the specified property type is nullable.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        private static bool IsNullable(Type propertyType) {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        protected T CreateModel<T>(DbDataReader dataReader) where T : class, new() {
            return CreateModels<T>(dataReader).FirstOrDefault();
        }

        /// <summary>
        /// Creates the models.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        protected IEnumerable<T> CreateModels<T>(DbDataReader dataReader) where T : class, new()
        {
            while (dataReader.Read()) {
                yield return CreateSingleModel<T>(dataReader);
            }
        }

        /// <summary>
        /// Creates the primitives collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        protected IEnumerable<T> CreatePrimitivesCollection<T>(DbDataReader dataReader) where T : struct {
            while (dataReader.Read()) {
                yield return (T)dataReader.GetValue(0);
            }
        }

        /// <summary>
        /// Creates the strings collection.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        protected IEnumerable<string> CreateStringsCollection(DbDataReader dataReader) {
            while (dataReader.Read()) {
                yield return (string)dataReader.GetValue(0);
            }
        }

        /// <summary>
        /// Creates the specified model from data reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        private T CreateSingleModel<T>(DbDataReader dataReader) where T : class, new() {
            T instance = new T();

            PropertyInfo[] properties = ObtainAllTypeProperties<T>();

            for (int i = 0; i < dataReader.FieldCount; ++i) {
                PropertyInfo property = FindWritablePropertyByName(properties, dataReader.GetName(i));
                if (property != null) {
                    Type propertyType = property.PropertyType;
                    object propertyValue = dataReader.GetValue(i);
                    if (propertyValue is DBNull) {
                        Log.Debug("got DBNull");
                        continue;
                    }

                    Type nullableType = Nullable.GetUnderlyingType(propertyType);
                    if (nullableType != null) {
                        propertyType = nullableType;
                    }

                    if (Type.GetTypeCode(propertyType) == Type.GetTypeCode(dataReader.GetFieldType(i))) {
                        property.SetValue(instance, propertyValue);
                    } else {
                        Log.Warn("reader type not equal to property type");
                        object result = null;
                        try {
                            result = Convert.ChangeType(propertyValue, Type.GetTypeCode(propertyType));
                        } catch (Exception ex) {
                            Log.Error(string.Format("could not convert {0} to {1}", propertyValue.GetType()
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
        /// Gets the empty command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        protected SqlCommand GetEmptyCommand(SqlConnection connection) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;
            return command;
        }

        /// <summary>
        /// Gets the insert command. Assumes that model's properties names corresponds to table's columns names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="outputColumnName">Name of the output column.</param>
        /// <param name="isSkipColumn">The is skip column.</param>
        /// <returns></returns>
        protected Optional<SqlCommand> GetInsertCommand<T>(T model, SqlConnection connection, string tableName, string outputColumnName = null, Predicate<string> isSkipColumn = null) where T : class {

            if (isSkipColumn == null) {
                isSkipColumn = o => false;
            }
            
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            StringBuilder insert = new StringBuilder("INSERT INTO ");
            insert.Append(tableName);
            insert.Append(" (");
            StringBuilder values = new StringBuilder("VALUES (");
            PropertyInfo[] properties = ObtainAllTypeProperties<T>();
            bool isDirty = false;
            //we assume that properties of a model that are stored in DB should be writable and readable
            foreach (var propertyInfo in properties.Where(prop => prop.CanWrite && prop.CanRead)) {

                if (isSkipColumn(propertyInfo.Name)) {
                    continue;
                }

                //this works also with nullable. GetValue gets null if nullable is empty
                object propertyValue = propertyInfo.GetValue(model);

                if (propertyValue != null) {

                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        propertyValue = Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(propertyValue.GetType()));
                    }

                    insert.Append(propertyInfo.Name);
                    insert.Append(comma); //adds comma

                    string param = strudel + propertyInfo.Name;
                    values.Append(param);
                    values.Append(comma); //adds comma

                    command.Parameters.AddWithValue(param, propertyValue);

                    isDirty = true;
                }
            }

            if (!isDirty) {
                return Optional<SqlCommand>.Empty();
            }

            insert.Remove(insert.Length - 1, 1); //removes last comma
            insert.Append(") ");

            if (!string.IsNullOrEmpty(outputColumnName))
            {
                insert.AppendLine();
                insert.Append("OUTPUT INSERTED.");
                insert.Append(outputColumnName);
                insert.AppendLine();
            }

            values.Remove(values.Length - 1, 1); //removes last comma
            values.Append(");");

          

            insert.Append(values);


            command.CommandText = insert.ToString();

            return Optional<SqlCommand>.Of(command);
        }

        /// <summary>
        /// Gets the multi insert command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">The models.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="isSkipColumn">The is skip column.</param>
        /// <example>
        /// INSERT INTO Table ( Column1, Column2 ) VALUES ( Value1, Value2 ), ( Value1, Value2 )
        /// </example>
        /// <returns></returns>
        protected Optional<SqlCommand> GetMultiInsertCommand<T>(IEnumerable<T> models, SqlConnection connection, string tableName, Predicate<string> isSkipColumn = null) where T : class {
            if (isSkipColumn == null)
            {
                isSkipColumn = o => false;
            }

            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            StringBuilder insert = new StringBuilder("INSERT INTO ");
            insert.Append(tableName);
            insert.Append(" (");
            StringBuilder values = new StringBuilder("VALUES ");
            PropertyInfo[] properties = ObtainAllTypeProperties<T>();
            bool isDirty = false;
            //we assume that properties of a model that are stored in DB should be writable and readable
            var relevantProperties = properties.Where(prop => prop.CanWrite && prop.CanRead && !isSkipColumn(prop.Name)).ToArray();
            
            //create columns statement
            foreach (var propertyInfo in relevantProperties)
            {
                insert.Append(propertyInfo.Name);
                insert.Append(comma);
            }

            insert.Remove(insert.Length - 1, 1); //removes last comma
            insert.Append(") ");

            int cnt = 0;
            foreach (var model in models) {
                ++cnt;
                //create values statement
                values.Append("(");
                foreach (var propertyInfo in relevantProperties) {
                    string param = strudel + propertyInfo.Name + cnt;
                    values.Append(param);
                    values.Append(comma); //adds comma

                    //this works also with nullable. GetValue gets null if nullable is empty
                    object propertyValue = propertyInfo.GetValue(model);
                    if (propertyValue != null) {
                        if (propertyInfo.PropertyType.IsEnum) {
                            propertyValue = Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(propertyValue.GetType()));
                        }
                    } else {
                        propertyValue = DBNull.Value;
                    }
                    
                    command.Parameters.AddWithValue(param, propertyValue);
                    isDirty = true;
                }
                values.Remove(values.Length - 1, 1); //removes last comma
                values.Append("),");
            }

            if (!isDirty)
            {
                return Optional<SqlCommand>.Empty();
            }

            values.Remove(values.Length - 1, 1); //removes last comma
            insert.Append(values);


            command.CommandText = insert.ToString();

            return Optional<SqlCommand>.Of(command);
        }

        /// <summary>
        /// Gets the upsert generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> GetUpsertGenerator<T>(T model) where T : class {
            return new UpsertCommandGenerator<T>(model);
        }

        /// <summary>
        /// Gets the upsert command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="outputColumnName">Name of the output column.</param>
        /// <returns></returns>
        /*
            MERGE SomeTable AS TARGET
            USING (VALUES(@Id,@Name,@LastName))
                AS SOURCE (Id,Name,LastName)
                ON (TARGET.Id = @Id AND TARGET.Name = @Name)
            WHEN MATCHED THEN
            UPDATE
                SET 
                Id = SOURCE.Id,
                Name = SOURCE.Name,
                LastName = SOURCE.LastName
            WHEN NOT MATCHED THEN INSERT (LastName,Id,Name)
            VALUES ( SOURCE.LastName,@Id,@Name)
            OUTPUT TARGET.Id
         */
        [Obsolete("all places that using this command should instead use UpsertCommandGenerator")]
        protected Optional<SqlCommand> GetUpsertCommand<T>(T model, SqlConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> ids, string outputColumnName = null, Predicate<string> skipColumn = null) where T : class
        {

            if (ids == null || !ids.Any()) {
                return GetInsertCommand(model, connection, tableName, outputColumnName);
            }

            if (skipColumn == null) {
                skipColumn = o => false;
            }

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = connection;
            sqlCommand.CommandType = CommandType.Text;
            StringBuilder values = new StringBuilder("MERGE ");
            values.Append(tableName);
            values.Append(" AS TARGET")
                .AppendLine();
            values.Append("USING (VALUES(");

            StringBuilder source = new StringBuilder(" AS SOURCE (");

            StringBuilder update = new StringBuilder("WHEN MATCHED THEN")
                .AppendLine()
                .Append("UPDATE")
                .AppendLine();

            StringBuilder insert = new StringBuilder("WHEN NOT MATCHED THEN INSERT (");

            StringBuilder insertValues = new StringBuilder("VALUES (");


            PropertyInfo[] properties = ObtainAllTypeProperties<T>();
            update.Append("  SET ")
                .AppendLine();

            bool isDirty = false;

            //we assume that properties of a model that are stored in DB should be writable and readable
            foreach (var propertyInfo in properties.Where(prop => prop.CanWrite && prop.CanRead)) {
                // we handle ids separately
                if (ids.Any(o => o.Key.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase))) {
                    continue;
                }

                if (skipColumn(propertyInfo.Name)) {
                    continue;
                }

                //this works also with nullable. GetValue gets null if nullable is empty
                object propertyValue = propertyInfo.GetValue(model);
                if (propertyValue != null) {

                    if (propertyInfo.PropertyType.IsEnum) {
                        propertyValue = Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(propertyValue.GetType()));
                    }

                    isDirty = true;

                    string param = strudel + propertyInfo.Name;
                    values.Append(param);
                    values.Append(comma); //adds comma

                    source.Append(propertyInfo.Name);
                    source.Append(comma); //adds comma

                    update.Append("   ")
                        .Append(propertyInfo.Name)
                        .Append(" = SOURCE.")
                        .Append(propertyInfo.Name)
                        .Append(comma)
                        .AppendLine();

                    if (ids.Select(o => o.Key)
                        .Contains(propertyInfo.Name)) {
                        continue;
                    }

                    sqlCommand.Parameters.AddWithValue(param, propertyValue);

                    insert.Append(propertyInfo.Name)
                        .Append(comma);

                    insertValues.Append(" SOURCE.")
                        .Append(propertyInfo.Name)
                        .Append(comma);
                }
            }

            if (!isDirty) {
                return Optional<SqlCommand>.Empty();
            }

            update.Remove(update.Length - 3, 3); //removes newlines and last comma



            StringBuilder onTarget = new StringBuilder(" ON (");

            foreach (var id in ids) {

                onTarget.Append("TARGET.")
                .Append(id.Key)
                .Append(" = ")
                .Append(strudel)
                .Append(id.Key)
                .Append(and);

                sqlCommand.Parameters.AddWithValue(strudel + id.Key, id.Value);

                if (skipColumn(id.Key))
                {
                    continue;
                }

                insert.Append(id.Key)
                    .Append(comma);
                insertValues.Append(strudel)
                    .Append(id.Key)
                    .Append(comma);

                values.Append(strudel)
                    .Append(id.Key)
                    .Append(comma);

                source.Append(id.Key);
                source.Append(comma); //adds comma
            }

            source.Remove(source.Length - 1, 1); //removes last comma
            source.Append(")");

            onTarget.Remove(onTarget.Length - and.Length, and.Length); //removes last AND statement
            onTarget.Append(")");

            insert.Remove(insert.Length - 1, 1); //removes last comma
            insert.Append(")");

            insertValues.Remove(insertValues.Length - 1, 1); //removes last comma
            insertValues.Append(")");

            values.Remove(values.Length - 1, 1); //removes last comma
            values.Append("))");

            values.AppendLine()
                .Append(source);
            values.AppendLine()
                .Append(onTarget);
            values.AppendLine()
                .Append(update);
            values.AppendLine()
                .Append(insert);
            values.AppendLine()
                .Append(insertValues);

            if (!string.IsNullOrEmpty(outputColumnName)) {
                values.AppendLine();
                values.Append("OUTPUT INSERTED.");
                values.Append(outputColumnName);
            }

            values.Append(";");

            sqlCommand.CommandText = values.ToString();

            return Optional<SqlCommand>.Of(sqlCommand);
        }

        /// <summary>
        /// Gets the count where command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumn">The where column.</param>
        /// <param name="whereValue">The where value.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected SqlCommand GetCountWhereCommand(SqlConnection connection, string whereColumn, object whereValue, string tableName) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            command.CommandText = String.Format("SELECT COUNT(*) FROM {0} WHERE @WhereColumn = '@WhereValue'", tableName);
            command.Parameters.AddWithValue("@WhereColumn", whereColumn);
            command.Parameters.AddWithValue("@WhereValue", whereValue);
            
            return command;
        }

        /// <summary>
        /// Gets the count where command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumnValues">The where columns + values.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected SqlCommand GetCountWhereCommand(SqlConnection connection, IList<KeyValuePair<string, object>> whereColumnValues, string tableName) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            StringBuilder queryBuilder = new StringBuilder("SELECT COUNT(*) FROM ").Append(tableName).Append(" WHERE ");
            int counter = 1;
            foreach (var pair in whereColumnValues) {
                string columnNameParam = "@" + pair.Key;
                string columnValueParam = "@" + counter;
                command.Parameters.AddWithValue(columnNameParam, pair.Key);
                command.Parameters.AddWithValue(columnValueParam, pair.Value);
                queryBuilder.Append(columnNameParam)
                    .Append(" = ")
                    .Append(columnValueParam);

                if (counter > 1 && counter < whereColumnValues.Count) {
                    queryBuilder.Append(" AND ");
                }

                ++ counter;
            }

            command.CommandText = queryBuilder.ToString();

            return command;
        }

        /// <summary>
        /// Gets the select all command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumn">The where column.</param>
        /// <param name="whereValue">The where value.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected SqlCommand GetSelectAllByWhereCommand(SqlConnection connection, string whereColumn, object whereValue, string tableName) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            command.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = {2}", tableName, whereColumn, whereValue);
            return command;
        }

        /// <summary>
        /// Gets the select all by where command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumnsValues">The where columns values.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected SqlCommand GetSelectAllByWhereCommand(SqlConnection connection, IList<KeyValuePair<string, object>> whereColumnsValues, string tableName) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            StringBuilder queryBuilder = new StringBuilder("SELECT * FROM " + tableName + " WHERE ");

            for (int i = 0; i < whereColumnsValues.Count; ++i) {
                string columnNameParam = "@" + whereColumnsValues[i].Key;
                string columnValueParam = "@" + (i + 1);
                command.Parameters.AddWithValue(columnNameParam, whereColumnsValues[i].Key);
                command.Parameters.AddWithValue(columnValueParam, whereColumnsValues[i].Value);
                queryBuilder.Append(columnNameParam)
                    .Append(" = ")
                    .Append(columnValueParam);

                if (i > 0 && i < whereColumnsValues.Count - 1)
                {
                    queryBuilder.Append(" AND ");
                }
            }

            return command;
        }

        /// <summary>
        /// Selects first matching row.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumn">The where column.</param>
        /// <param name="whereValue">The where value.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected SqlCommand GetSelectFirstByWhereCommand(SqlConnection connection, string whereColumn, object whereValue, string tableName) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            string commandText = whereValue == null ? 
                string.Format("SELECT TOP 1 * FROM {0} WHERE {1} IS NULL", tableName, whereColumn) :
                string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = @WhereValue", tableName, whereColumn);

            command.CommandText = commandText;
            if (whereValue != null) {
                command.Parameters.AddWithValue("@WhereValue", whereValue);
            }

            return command;
        }

        /// <summary>
        /// Gets the select fist by where command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="whereColumnValues">The where column values.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="outputColumn">The output column.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">empty where column-values collection</exception>
        protected SqlCommand GetSelectFirstByWhereCommand(SqlConnection connection, IEnumerable<KeyValuePair<string, object>> whereColumnValues, string tableName, string outputColumn = null) {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;

            if (CollectionUtils.IsEmpty(whereColumnValues)) {
                throw new ArgumentException("empty where column-values collection");
            }

            if (string.IsNullOrEmpty(tableName)) {
                throw new ArgumentException("empty table name");
            }



            StringBuilder sb;
            if (string.IsNullOrEmpty(outputColumn)) {
                sb = new StringBuilder("SELECT TOP 1 * FROM ");
            } else {
                sb = new StringBuilder("SELECT TOP 1 ")
                    .Append(outputColumn)
                    .Append(" FROM ");
            }

            sb.Append(tableName);
            sb.Append(" WHERE ");
            foreach (var columnValue in whereColumnValues) {
                if (columnValue.Value != null) {
                    sb.Append(columnValue.Key);
                    sb.Append(" = ");
                    sb.Append(strudel + columnValue.Key);

                    command.Parameters.AddWithValue(strudel + columnValue.Key, columnValue.Value);
                } else {
                    sb.Append(columnValue.Key);
                    sb.Append(" IS NULL");
                }

                sb.Append(" AND ");
            }

            sb.Remove(sb.Length - 5, 5); //removes last 'AND'

            command.CommandText = sb.ToString();
            return command;
        }

        /// <summary>
        /// Executes the commands and logs errors if there are.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected bool ExecuteNonQueryAndLog(SqlCommand command) {
            try {
                int numberOfRowsAffected = command.ExecuteNonQuery();

                if (numberOfRowsAffected < 1) {
                    Log.Error("got error while executing query");
                    return false;
                }
            } catch (SqlException ex) {
                Log.Error(ex);
                throw;
            }

            return true;
        }

        /// <summary>
        /// Executes the scalar and log.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected Optional<T> ExecuteScalarAndLog<T>(SqlCommand command) {
            try {
                object res = command.ExecuteScalar();
                if (res == DBNull.Value || res == null) {
                    return Optional<T>.Empty();
                }
                return Optional<T>.Of((T)res);

            } catch (SqlException ex) {
                Log.Error(ex);
                throw;
            } catch (InvalidCastException ex) {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Skips the columns.
        /// </summary>
        /// <param name="columnNames">The column names.</param>
        /// <returns></returns>
        protected Predicate<string> SkipColumns(params string[] columnNames) {
            if (CollectionUtils.IsEmpty(columnNames)) {
                return o => false;
            }

            if (columnNames.Any(string.IsNullOrEmpty)) {
                throw new ArgumentException("got null or empty column");
            }

            return o => columnNames.Any(c => c.Equals(o, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Obtains the type public properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PropertyInfo[] ObtainTypePublicProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Obtains all properties of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PropertyInfo[] ObtainAllTypeProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Finds the writable property by name.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="dbPropertyName">Name of the property in DB.</param>
        /// <returns></returns>
        private PropertyInfo FindWritablePropertyByName(PropertyInfo[] properties, string dbPropertyName) {
            return properties.Where(prop => prop.CanWrite)
                .FirstOrDefault(property => string.Equals(property.Name, dbPropertyName, StringComparison.InvariantCultureIgnoreCase));
//            foreach (var property in properties.Where(prop => prop.CanWrite)) {
//                int indexOf = CultureInfo.InvariantCulture.CompareInfo.IndexOf(dbPropertyName, property.Name, CompareOptions.IgnoreCase);
//                if (indexOf > -1) {
//                    return property;
//                }
//
//                indexOf = CultureInfo.InvariantCulture.CompareInfo.IndexOf(property.Name, dbPropertyName, CompareOptions.IgnoreCase);
//                if (indexOf > -1) {
//                    return property;
//                }
//            }
        }
    }
}
