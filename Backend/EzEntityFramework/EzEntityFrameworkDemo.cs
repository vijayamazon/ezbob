namespace EzEntityFramework
{
	using System;
	using System.Configuration;
	using System.Data.Entity;
	using System.Data.EntityClient;
	using System.Data.SqlClient;
	using Environment = Ezbob.Context.Environment;

	public partial class ezbobEntities : DbContext
	{
		public ezbobEntities(string connectionString)
            : base(connectionString)
        {
		}
	}

	public class EzEntityFrameworkDemo
    {
		public string GetConnectionString(Environment environment)
		{
			string connectionStringConfig = ConfigurationManager.ConnectionStrings[environment.Context.ToLower()].ConnectionString;

			string dataSource = ExtractPart("server", connectionStringConfig);
			string initialCatalog = ExtractPart("database", connectionStringConfig);
			string userId = ExtractPart("user id", connectionStringConfig);
			string password = ExtractPart("password", connectionStringConfig);
			string connectTimeoutStr = ExtractPart("connection timeout", connectionStringConfig);
			int connectTimeout;
			string multipleActiveResultSets = ExtractPart("MultipleActiveResultSets", connectionStringConfig);

			var sqlBuilder = new SqlConnectionStringBuilder();
			sqlBuilder.DataSource = dataSource;
			sqlBuilder.InitialCatalog = initialCatalog;
			sqlBuilder.UserID = userId;
			sqlBuilder.Password = password;
			if (multipleActiveResultSets == "true")
			{
				sqlBuilder.MultipleActiveResultSets = true;
			}
			if (int.TryParse(connectTimeoutStr, out connectTimeout))
			{
				sqlBuilder.ConnectTimeout = connectTimeout;
			}
			//sqlBuilder.IntegratedSecurity = true;
			string providerString = sqlBuilder.ToString();

			var entityBuilder = new EntityConnectionStringBuilder();
			entityBuilder.Provider = "System.Data.SqlClient";
			entityBuilder.ProviderConnectionString = providerString;
			entityBuilder.Metadata = @"res://*/EzbobModel.csdl|res://*/EzbobModel.ssdl|res://*/EzbobModel.msl";

			return entityBuilder.ToString();
		}

		private string ExtractPart(string partName, string connectionStringConfig)
		{
			int partStartIndex = connectionStringConfig.IndexOf(partName, StringComparison.Ordinal);

			if (partStartIndex == -1)
			{
				return null;
			}

			int partEndIndex = connectionStringConfig.Substring(partStartIndex).IndexOf(";", StringComparison.Ordinal);
			int actualPosition = partStartIndex + partName.Length;
			if (partEndIndex == -1)
			{
				return connectionStringConfig.Substring(actualPosition + 1);
			}
			return connectionStringConfig.Substring(actualPosition + 1, partStartIndex + partEndIndex - actualPosition - 1);
		}
    }
}
