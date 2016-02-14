using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EzBobPersistence.QueryGenerators
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq.Expressions;
    using EzBobCommon;

    /// <summary>
    /// Generates query like: SELECT col1, col2 FROM TABLE WHERE id = 1 && name='a'
    /// </summary>
    /// <typeparam name="T">the type of model</typeparam>
    class SelectWhereGenerator<T> : CommandGeneratorBase where T : class {
        private readonly Dictionary<string, object> where = new Dictionary<string, object>();
        private string[] columns;
        private SqlConnection sqlConnection;
        private string table;
        private bool isSelectAll = true;


        /// <summary>
        /// Set the where columns (with AND operator).
        /// </summary>
        /// <param name="whereProperty">The where property access expression.</param>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public SelectWhereGenerator<T> WithWhere(Expression<Func<T, object>> whereProperty, object val) {
            string colName = ExtractMemberName(whereProperty);
            if (val != null) {
                this.where[colName] = val;
            }

            return this;
        }

        /// <summary>
        /// Sets the select columns.(If called without parameters assumes SELECT *)
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public SelectWhereGenerator<T> WithSelect(params Expression<Func<T, object>>[] properties) {
            if (properties.IsEmpty()) {
                this.isSelectAll = true;
                return this;
            }

            this.columns = properties.Select(ExtractMemberName)
                .Distinct().ToArray();

            this.isSelectAll = false;

            return this;
        }

        /// <summary>
        /// Sets the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public SelectWhereGenerator<T> WithTableName(string tableName) {
            this.table = tableName;
            return this;
        }

        /// <summary>
        /// Sets the optional connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public SelectWhereGenerator<T> WithOptionalConnection(SqlConnection connection) {
            this.sqlConnection = connection;
            return this;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Empty table name.
        /// or
        /// No where arguments is specified
        /// </exception>
        public override ISqlCommandGenerator Verify() {
            if (this.table.IsEmpty()) {
                throw new ArgumentException("Empty table name.");
            }

            if (this.where.Count == 0) {
                throw new ArgumentException("No where arguments is specified");
            }

            return base.GetGenerator();

        }

        /// <summary>
        /// Generates the command.
        /// </summary>
        /// <returns></returns>
        protected override SqlCommand GenerateCommand() {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            if (this.sqlConnection != null) {
                command.Connection = this.sqlConnection;
            }

            StringBuilder b = new StringBuilder("SELECT ");
            if (this.isSelectAll) {
                b.Append("*");
            } else {
                for (int i = 0; i < this.columns.Length; ++i) {
                    if (i > 0) {
                        b.Append(", ");
                    }
                    b.Append(this.columns[i]);
                }
            }
            b.Append(" FROM ")
                .Append(this.table)
                .Append(" WHERE ");

            int cnt = 0;
            foreach (var keyValue in this.where) {
                if (cnt > 0) {
                    b.Append(" AND ");
                }

                b.Append(keyValue.Key)
                    .Append(" = ")
                    .Append("@")
                    .Append(keyValue.Key);

                command.Parameters.AddWithValue("@" + keyValue.Key, keyValue.Value);

                ++ cnt;
            }

            command.CommandText = b.ToString();
            return command;
        }
    }
}
