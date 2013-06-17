namespace DbConnection
{
	using System;
	using System.Data;
	using System.Data.SqlClient;
	using System.Diagnostics;
	using System.Text;
	using System.Threading;
	using Scorto.Configuration;
	using log4net;
	using log4net.Config;

	public class DbConnection
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(DbConnection));
		private static readonly string connectionString;

        static DbConnection()
        {
			var configuration = ConfigurationRoot.GetConfiguration();
			XmlConfigurator.Configure(configuration.XmlElementLog);
			DbLibConfiguration dbLibConfiguration = configuration.DbLib;

	        connectionString = dbLibConfiguration.ConnectionString;
			Log.Info(string.Format("ConnectionString: {0}", connectionString));
        }

        private static T Retry<T>(Func<T> func)
        {
            int count = 3;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (SqlException e)
                {
                    --count;
                    if (count <= 0)
                    {
                        throw;
                    }
                    
                    if (e.Number == 1205)
                    {
						Log.Warn(string.Format("Deadlock, retrying {0}", e));
                    }
                    else if (e.Number == -2)
                    {
						Log.Warn(string.Format("Timeout, retrying {0}", e));
                    }
                    else
                    {
                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }

        private static void Retry(Action action)
        {
            Retry(() => { action(); return true; });
        }

        public static T ExecuteScalar<T>(string queryString)
        {
			Log.Debug(string.Format("Starting to run query:{0}", queryString));

            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(queryString, connection))
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            object value = command.ExecuteScalar();
							Log.Debug(string.Format("Finished query in {0}ms", sw.ElapsedMilliseconds));
                            if (value is DBNull) return default(T);
                            return (T)value;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for query:'{0}'\nThe Error:'{1}'", queryString, e));
                throw;
            }
        }

        public static T ExecuteSpScalar<T>(string spName, params SqlParameter[] sqlParams)
        {
            var sb = new StringBuilder();
            foreach (SqlParameter currentParam in sqlParams)
            {
                sb.Append(sb.ToString() != string.Empty ? ", " : " with parameters:");
                sb.Append(currentParam.ParameterName).Append("=").Append(currentParam.Value);
            }
			Log.Debug(string.Format("Starting to run sp:{0}{1}", spName, sb));

            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(spName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddRange(sqlParams);
                            var sw = new Stopwatch();
                            sw.Start();
                            object value = command.ExecuteScalar();
							Log.Debug(string.Format("Finished sp in {0}ms", sw.ElapsedMilliseconds));
                            if (value is DBNull) return default(T);
                            return (T)value;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for sp:'{0}'\nThe Error:'{1}'", spName, e));
                throw;
            }
        }

        public static DataTable ExecuteReader(string queryString)
        {
			Log.Debug(string.Format("Starting to run query:{0}", queryString));
            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(queryString, connection))
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            SqlDataReader sqlDataReader = command.ExecuteReader();
							Log.Debug(string.Format("Finished query in {0}ms", sw.ElapsedMilliseconds));
                            var dataTable = new DataTable();
                            dataTable.Load(sqlDataReader);
                            return dataTable;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for query:'{0}'\nThe Error:'{1}'", queryString, e));
                throw;
            }
        }

        public static DataTable ExecuteSpReader(string spName, params SqlParameter[] sqlParams)
        {
            var sb = new StringBuilder();
            foreach (SqlParameter currentParam in sqlParams)
            {
                sb.Append(sb.ToString() != string.Empty ? ", " : " with parameters:");
                sb.Append(currentParam.ParameterName).Append("=").Append(currentParam.Value);
            }
			Log.Debug(string.Format("Starting to run sp:{0}{1}", spName, sb));

            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(spName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddRange(sqlParams);
                            var sw = new Stopwatch();
                            sw.Start();
                            SqlDataReader sqlDataReader = command.ExecuteReader();
							Log.Debug(string.Format("Finished sp in {0}ms", sw.ElapsedMilliseconds)); 
                            var dataTable = new DataTable();
                            dataTable.Load(sqlDataReader);
                            return dataTable;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for sp:'{0}'\nThe Error:'{1}'", spName, e));
                throw;
            }
        }

        public static int ExecuteNonQuery(string queryString)
        {
			Log.Debug(string.Format("Starting to run query:{0}", queryString));
            
            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(queryString, connection))
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            int result = command.ExecuteNonQuery();
							Log.Debug(string.Format("Finished query in {0}ms", sw.ElapsedMilliseconds));
                            return result;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for query:'{0}'\nThe Error:'{1}'", queryString, e));
                throw;
            }
        }

        public static int ExecuteSpNonQuery(string spName, params SqlParameter[] sqlParams)
        {
            var sb = new StringBuilder();
            foreach (SqlParameter currentParam in sqlParams)
            {
                sb.Append(sb.ToString() != string.Empty ? ", " : " with parameters:");
                sb.Append(currentParam.ParameterName).Append("=").Append(currentParam.Value);
            }
			Log.Debug(string.Format("Starting to run sp:{0}{1}", spName, sb));

            try
            {
                return Retry(() =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand(spName, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddRange(sqlParams);
                            var sw = new Stopwatch();
                            sw.Start();
                            int result = command.ExecuteNonQuery();
							Log.Debug(string.Format("Finished sp in {0}ms", sw.ElapsedMilliseconds));
                            return result;
                        }
                    }
                });
            }
            catch (Exception e)
            {
				Log.Error(string.Format("SQL error for sp:'{0}'\nThe Error:'{1}'", spName, e));
                throw;
            }
        }

        public static SqlParameter CreateParam(string name, object value)
        {
            if (value is int && (int)value == 0)
            {
                return new SqlParameter(name, Convert.ToInt32(0));
            }

            return new SqlParameter(name, value);
        }
    }
}
