using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EzBobPersistence.QueryGenerators {
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq.Expressions;
    using System.Reflection;
    using EzBobCommon;
    using EzBobCommon.Utils;

    /// <summary>
    /// Generates multi insert command.
    /// </summary>
    /// <example>
    /// INSERT INTO Table ( Column1, Column2 ) VALUES ( Value1, Value2 ), ( Value1, Value2 )
    /// OUTPUT INSERTED.somColumn
    /// </example>
    /// <typeparam name="T"></typeparam>
    internal class MultiInsertCommandGenerator<T>
        where T : class, new() {
        private static readonly string comma = ",";
        private static readonly string strudel = "@";
        private static readonly string and = " AND ";

        private IEnumerable<T> models;
        private ISet<string> skipColumns = new HashSet<string>();
        private SqlConnection connection;
        private string tableName;
        private Predicate<string> isSkipColumn = o => false;
        private ISet<string> outputColumns = new HashSet<string>();
        private bool isOutputAll = false;

        public class Generator<TT> : ISqlCommandsGenerator
            where TT : class, new() {
            private MultiInsertCommandGenerator<TT> generator;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Generator(MultiInsertCommandGenerator<TT> generator) {
                this.generator = generator;
            }

            public IEnumerable<SqlCommand> GenerateCommands() {
                return this.generator.GenerateCommands()
                    .ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MultiInsertCommandGenerator() {}

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MultiInsertCommandGenerator(IEnumerable<T> models) {
            this.models = models;
        }

        /// <summary>
        /// Sets the models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        public MultiInsertCommandGenerator<T> WithModels(IEnumerable<T> models) {
            this.models = models;
            return this;
        }

        /// <summary>
        /// Sets the optional connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public MultiInsertCommandGenerator<T> WithOptionalConnection(SqlConnection connection) {
            this.connection = connection;
            return this;
        }

        /// <summary>
        /// Sets the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public MultiInsertCommandGenerator<T> WithTableName(string tableName) {
            this.tableName = tableName;
            return this;
        }

        /// <summary>
        /// Sets the columns to skip.
        /// </summary>
        /// <param name="skipColumnNames">The skip columns.</param>
        /// <returns></returns>
        [Obsolete("try to use the expression based overload")]
        public MultiInsertCommandGenerator<T> WithSkipColumns(params string[] skipColumnNames) {
            this.skipColumns.UnionWith(skipColumnNames);
            return this;
        }

        /// <summary>
        /// Sets the skip column name. The name is taken from provided property access expression.
        /// <remarks>
        /// WithSkipKolumn(o =&gt; o.Age) ---&gt; understands that column name is 'Age'
        /// </remarks>
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">expected lambda like o => o.Age</exception>
        public MultiInsertCommandGenerator<T> WithSkipColumns(params Expression<Func<T, object>>[] propertyAccessExpressions) {
            IEnumerable<string> colNames = this.ExtractMemberNames(propertyAccessExpressions);
            this.skipColumns.UnionWith(colNames);
            return this;
        }

        /// <summary>
        /// Sets the output columns.
        /// </summary>
        /// <param name="outputColumnNames">The output column names.</param>
        /// <returns></returns>
        [Obsolete("try to use the expression based overload")]
        public MultiInsertCommandGenerator<T> WithOutputColumns(params string[] outputColumnNames) {
            this.outputColumns.UnionWith(outputColumnNames);
            return this;
        }

        /// <summary>
        /// Sets the output columns names. The names are deduced from provided property access expressions
        /// </summary>
        /// <param name="propertyAccessExpressions">The property access expressions.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">expected lambda like o =&gt; o.Age</exception>
        public MultiInsertCommandGenerator<T> WithOutputColumns(params Expression<Func<T, object>>[] propertyAccessExpressions) {
            IEnumerable<string> colNames = this.ExtractMemberNames(propertyAccessExpressions);
            this.outputColumns.UnionWith(colNames);
            return this;
        }

        /// <summary>
        /// Set the output all (output inserted.*).
        /// </summary>
        /// <returns></returns>
        public MultiInsertCommandGenerator<T> WithOutputAll() {
            this.isOutputAll = true;
            return this;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// empty table name
        /// or
        /// empty collection of models
        /// </exception>
        public ISqlCommandsGenerator Verify() {
            if (StringUtils.IsEmpty(this.tableName)) {
                throw new ArgumentException("empty table name");
            }

            if (this.models.IsEmpty()) {
                throw new ArgumentException("empty collection of models");
            }

            return new Generator<T>(this);
        }

        /// <summary>
        /// Generates the commands. Multiple commands, because if there are too many models, they will be divided into batches.
        /// Each command will insert one batch.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SqlCommand> GenerateCommands() {

            if (this.skipColumns.IsNotEmpty()) {
                this.isSkipColumn = s => this.skipColumns.Any(o => o.Equals(s, StringComparison.CurrentCultureIgnoreCase));
            }

            PropertyInfo[] properties = ObtainAllTypeProperties();
            bool isDirty = false;
            //we assume that properties of a model that are stored in DB should be writable and readable
            var relevantProperties = properties.Where(prop => prop.CanWrite && prop.CanRead && !isSkipColumn(prop.Name))
                .ToArray();

            foreach (var batch in this.models.Batch(800)) {
                SqlCommand command = new SqlCommand();
                command.CommandType = CommandType.Text;
                command.Connection = connection;

                StringBuilder insert = new StringBuilder("INSERT INTO ");
                insert.Append(tableName);
                insert.Append(" (");
                StringBuilder values = new StringBuilder("VALUES ");


                //create columns statement
                foreach (var propertyInfo in relevantProperties) {
                    insert.Append(propertyInfo.Name);
                    insert.Append(comma);
                }

                insert.Remove(insert.Length - 1, 1); //removes last comma
                insert.Append(") ");

                int cnt = 0;
                foreach (var model in batch) {
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

                if (!isDirty) {
                    continue;
                }

                values.Remove(values.Length - 1, 1); //removes last comma
                insert.Append(values);

                if (this.isOutputAll) {
                    insert.AppendLine();
                    insert.Append(" OUTPUT INSERTED.*");
                } else if (this.outputColumns.IsNotEmpty()) {
                    insert.AppendLine();
                    insert.Append(" OUTPUT");
                    foreach (string col in this.outputColumns) {
                        insert.Append(" INSERTED.");
                        insert.Append(col);
                        insert.Append(comma);
                    }

                    insert.Remove(insert.Length - 1, 1); //removes last comma
                }


                command.CommandText = insert.ToString();
                yield return command;
            }
        }

        /// <summary>
        /// Obtains all properties of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PropertyInfo[] ObtainAllTypeProperties() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Extracts the member names.
        /// </summary>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">expected lambda like o => o.Age</exception>
        private IEnumerable<string> ExtractMemberNames(IEnumerable<Expression<Func<T, object>>> expressions) {
            foreach (var propertyAccessExpression in expressions) {
                yield return ExtractMemberName(propertyAccessExpression);
            }
        }

        /// <summary>
        /// Extracts the name of the member.
        /// </summary>
        /// <typeparam name="V">represents any property type</typeparam>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">expected lambda like o => o.Age</exception>
        private string ExtractMemberName(Expression<Func<T, object>> propertyAccessExpression) {
            MemberExpression memberExpression = propertyAccessExpression.Body as MemberExpression;

            if (memberExpression == null) {
                //it could be a Convert expression, for example, if property type is struct
                UnaryExpression u = propertyAccessExpression.Body as UnaryExpression;
                if (u != null) {
                    memberExpression = u.Operand as MemberExpression;
                }
            }

            if (memberExpression == null) {
                throw new ArgumentException("expected lambda like o => o.Age");
            }

            return memberExpression.Member.Name;
        }
    }
}
