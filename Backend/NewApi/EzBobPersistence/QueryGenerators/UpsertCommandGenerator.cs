namespace EzBobPersistence.QueryGenerators {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;


    /// <summary>
    /// Generates upsert command (MERGE)<br/>
    /// To generate command call Verify() and then GenerateCommand()
    /// <remarks>
    /// Methods which begin from 'ADD' - optional, from 'SET' - mandatory
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpsertCommandGenerator<T> : CommandGeneratorBase
        where T : class {
        private static readonly string comma = ",";
        private static readonly string strudel = "@";
        private static readonly string and = " AND ";

        private readonly T model;
        private string tableName;
        private IEnumerable<string> matchingColumnNames;
        private IEnumerable<string> updateIfNull = Enumerable.Empty<string>();
        private IEnumerable<string> outputColumns = Enumerable.Empty<string>();
        private Predicate<string> skipColumn = o => false;
        private bool isOutputAll;

        private SqlConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertCommandGenerator{T}"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public UpsertCommandGenerator(T model) {
            this.model = model;
        }

        /// <summary>
        /// Adds the connection.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithConnection(SqlConnection sqlConnection) {
            this.connection = sqlConnection;
            return this;
        }

        /// <summary>
        /// Sets the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithTableName(string tableName) {
            this.tableName = tableName;
            return this;
        }

        /// <summary>
        /// Sets matching column names
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithMatchColumns(params Expression<Func<T, object>>[] propertyAccessExpressions) {
            this.matchingColumnNames = propertyAccessExpressions.Select(ExtractMemberName);
            return this;
        }

        /// <summary>
        /// Adds the update column if null.
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithUpdateColumnIfNull(params Expression<Func<T, object>>[] propertyAccessExpressions) {
            this.updateIfNull = propertyAccessExpressions.Select(ExtractMemberName)
                .ToArray();

            return this;
        }

        /// <summary>
        /// Adds the output columns.
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithOutputColumns(params Expression<Func<T, object>>[] propertyAccessExpressions) {
            var columns = propertyAccessExpressions.Select(ExtractMemberName)
                .ToArray();

            if (!columns.Any()) {
                this.isOutputAll = true;
            } else {
                this.isOutputAll = false;
                this.outputColumns = columns;
            }
            return this;
        }

        /// <summary>
        /// Adds the skip columns.
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        public UpsertCommandGenerator<T> WithSkipColumns(params Expression<Func<T, object>>[] propertyAccessExpressions) {

            var columns = propertyAccessExpressions.Select(ExtractMemberName)
                .ToArray();

            if (columns.Any()) {
                this.skipColumn = c => columns.Any(o => c.Equals(o, StringComparison.CurrentCultureIgnoreCase));
            } else {
                this.skipColumn = o => false;
            }

            return this;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// empty table name
        /// or
        /// empty model
        /// or
        /// empty matching column values
        /// </exception>
        public override ISqlCommandGenerator Verify() {
            if (string.IsNullOrEmpty(this.tableName)) {
                throw new ArgumentException("empty table name");
            }

            if (this.model == null) {
                throw new ArgumentException("empty model");
            }

            if (this.matchingColumnNames == null || !this.matchingColumnNames.Any()) {
                throw new ArgumentException("empty matching column names");
            }

            return base.GetGenerator();
        }

        /// <summary>
        /// Generates the command.
        /// </summary>
        /// <returns></returns>
        protected override SqlCommand GenerateCommand() {
            return GetUpsertCommand();
        }

        protected SqlCommand GetUpsertCommand() {
            SqlCommand sqlCommand = new SqlCommand();
            if (this.connection != null) {
                sqlCommand.Connection = this.connection;
            }
            sqlCommand.CommandType = CommandType.Text;

            StringBuilder values = new StringBuilder("MERGE ");
            values.Append(this.tableName);
            values.Append(" AS TARGET")
                .AppendLine();
            values.Append("USING (VALUES(");

            StringBuilder source = new StringBuilder(" AS SOURCE (");

            StringBuilder update = new StringBuilder("WHEN MATCHED THEN")
                .AppendLine()
                .Append("UPDATE")
                .AppendLine()
                .Append("  SET ")
                .AppendLine();

            StringBuilder insert = new StringBuilder("WHEN NOT MATCHED THEN INSERT (");

            StringBuilder insertValues = new StringBuilder("VALUES (");
            PropertyInfo[] properties = ObtainAllTypeProperties<T>();

            bool isDirty = false;

            //we assume that properties of a model that are stored in DB should be writable and readable
            foreach (var propertyInfo in properties.Where(prop => prop.CanWrite && prop.CanRead)) {


                //this works also with nullable. GetValue gets null if nullable is empty
                object propertyValue = propertyInfo.GetValue(this.model);
                if (propertyValue != null) {

                    if (propertyInfo.PropertyType.IsEnum) {
                        propertyValue = Convert.ChangeType(propertyValue,
                            Enum.GetUnderlyingType(propertyValue.GetType()));
                    }

                    isDirty = true;

                    string param = strudel + propertyInfo.Name;
                    sqlCommand.Parameters.AddWithValue(param, propertyValue);

                    this.HandleMergeValues(values, param);
                    this.HandleMergeSource(source, propertyInfo.Name);
                    
                    if (this.skipColumn(propertyInfo.Name)) {
                        continue;
                    }
                    
                    this.HandleUpdate(update, propertyInfo.Name);
                    this.HandleWhenNotMatchThenInsertToColumns(insert, propertyInfo.Name);
                    this.HandleWhenNotMatchThenInsertValues(insertValues, propertyInfo.Name);
                }
            }

            if (!isDirty) {
                return null;
            }

            update.Remove(update.Length - 3, 3); //removes newlines and last comma

            StringBuilder onTarget = new StringBuilder(" ON (");
            this.HandleOnTarget(onTarget);

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

            this.HandleOutputInserted(values);

            sqlCommand.CommandText = values.ToString();

            return sqlCommand;
        }

        /// <summary>
        /// Handles the output inserted.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void HandleOutputInserted(StringBuilder builder) {
            if (this.isOutputAll) {
                builder.AppendLine();
                builder.Append("OUTPUT INSERTED.*");
            } else if (this.outputColumns != null && this.outputColumns.Any()) {
                builder.AppendLine();
                builder.Append("OUTPUT");
                foreach (var outputColumName in this.outputColumns) {
                    builder.Append(" INSERTED.");
                    builder.Append(outputColumName);
                    builder.Append(",");
                }

                builder.Remove(builder.Length - 1, 1); //removes last comma
            }
            builder.Append(";");
        }

        /// <summary>
        /// Handles the on target.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void HandleOnTarget(StringBuilder builder) {
            foreach (var colName in this.matchingColumnNames.Distinct()) {

                builder.Append("TARGET.")
                    .Append(colName)
                    .Append(" = ")
                    .Append(strudel)
                    .Append(colName)
                    .Append(and);
            }
        }

        /// <summary>
        /// Handles the merge source.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="columName">Name of the colum.</param>
        private void HandleMergeSource(StringBuilder builder, string columName) {
            builder.Append(columName);
            builder.Append(comma); //adds comma
        }

        /// <summary>
        /// Handles the merge values.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="paramName">Name of the parameter.</param>
        private void HandleMergeValues(StringBuilder builder, string paramName) {
            builder.Append(paramName);
            builder.Append(comma); //adds comma
        }

        /// <summary>
        /// Handles the when not match then insert to columns.
        /// </summary>
        /// <param name="insert">The insert.</param>
        /// <param name="columnName">Name of the column.</param>
        private void HandleWhenNotMatchThenInsertToColumns(StringBuilder insert, string columnName) {
            insert.Append(columnName)
                .Append(comma);
        }

        /// <summary>
        /// Handles the "when not match then insert" part of 'merge' query.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="columnName">Name of the column.</param>
        private void HandleWhenNotMatchThenInsertValues(StringBuilder builder, string columnName) {
            builder.Append(" SOURCE.")
                .Append(columnName)
                .Append(comma);
        }

        /// <summary>
        /// Handles the 'update' part of 'merge' query.
        /// </summary>
        /// <param name="update">The update.</param>
        /// <param name="columnName">Name of the column.</param>
        private void HandleUpdate(StringBuilder update, string columnName) {
            if (!this.updateIfNull.Any(
                o => o.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))) {

                update.Append("   ")
                    .Append(columnName)
                    .Append(" = SOURCE.")
                    .Append(columnName)
                    .Append(comma)
                    .AppendLine();
            } else {
                update.Append("   ")
                    .Append(columnName)
                    .Append(" = ")
                    .Append("(CASE WHEN ") //CASE WHEN
                    .Append("TARGET.")
                    .Append(columnName) //TARGET.FIELDNAME
                    .Append(" IS NULL THEN") //IS NULL THEN SOURCE.FIELDNAME
                    .Append(" SOURCE.")
                    .Append(columnName)
                    .Append(" ELSE ")
                    .Append("TARGET.")
                    .Append(columnName) //ELSE TARGET.FIELDNAME
                    .Append(" END)")
                    .Append(comma)
                    .AppendLine();
            }
        }
    }
}
