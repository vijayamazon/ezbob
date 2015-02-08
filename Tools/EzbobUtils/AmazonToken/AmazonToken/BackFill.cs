using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace AmazonToken
{
    class BackFill
    {
		private static readonly SqlConnection connectionString = new SqlConnection("Server=localhost;Database=ezbob;User Id=stas;Password=ezbobuser;MultipleActiveResultSets=true;connection timeout=30");
        private readonly int retryNumber;
        public BackFill(int retryNumber=1)
        {
            this.retryNumber = retryNumber;
        }

        public void Execute()
        {
            try
            {
                connectionString.Open();

                SqlCommand sqlCommand = new SqlCommand("select mp.Id, cast(mp.SecurityData as varchar(max)) Data from MP_CustomerMarketPlace mp inner join MP_MarketplaceType t on mp.MarketPlaceId=t.Id where t.InternalId='A4920125-411F-4BB9-A52D-27E8A00D0A3B'", connectionString);
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();

				using (StreamWriter file = new StreamWriter(@"C:\Temp\AmazonAuth\results.sql"))
                {
                    while (sqlReader.Read())
                    {
                        XMLLineHandler currentLine = new XMLLineHandler(sqlReader["id"].ToString(), retryNumber);
                        currentLine.outputInfo = currentLine.ReadXMLLine(sqlReader["Data"].ToString());
                        file.WriteLine(currentLine.outputInfo);
                        Thread.Sleep(20 * 1000);
                    }
                }
                connectionString.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
