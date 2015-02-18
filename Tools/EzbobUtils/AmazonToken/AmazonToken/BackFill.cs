using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace AmazonToken
{
    class BackFill
    {
		private static readonly SqlConnection connectionString = new SqlConnection("user id=sa;password=Or@nge123;server=192.168.120.10;Trusted_Connection=no;database=ezbob;connection timeout=30;MultipleActiveResultSets=true");
        private readonly int retryNumber;
		private readonly int minId;

	    public BackFill(int minId, int retryNumber=1)
        {
            this.retryNumber = retryNumber;
	        this.minId = minId;
        }

        public void Execute()
        {
            try
            {
                connectionString.Open();

				SqlCommand sqlCommand = new SqlCommand(
														@"SELECT mp.Id, CAST(mp.SecurityData AS VARCHAR(max)) AS Data 
														FROM MP_CustomerMarketPlace mp 
														INNER JOIN MP_MarketplaceType t ON mp.MarketPlaceId=t.Id 
														WHERE 
															t.InternalId='A4920125-411F-4BB9-A52D-27E8A00D0A3B'
														AND 
															mp.Disabled=0
														AND 
															mp.DisplayName<>'Geltology inc'
														AND
															mp.Id >" + minId, connectionString);
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();

				using (StreamWriter file = new StreamWriter(@"C:\Temp\AmazonAuth\results.sql"))
                {
                    while (sqlReader.Read()) {
	                    try {
		                    file.WriteLine("-- begin of " + sqlReader["Id"].ToString() + " " + DateTime.Now);
		                    Console.WriteLine("-- begin of " + sqlReader["Id"].ToString() + " " + DateTime.Now);
		                    XMLLineHandler currentLine = new XMLLineHandler(sqlReader["Id"].ToString(), retryNumber);
		                    currentLine.outputInfo = currentLine.ReadXMLLine(sqlReader["Data"].ToString());
		                    file.WriteLine(currentLine.outputInfo);
		                    Console.WriteLine(currentLine.outputInfo);
		                    file.WriteLine("-- end of " + sqlReader["Id"].ToString() + " " + DateTime.Now + " sleeping 60 sec");
		                    Console.WriteLine("-- end of " + sqlReader["Id"].ToString() + " " + DateTime.Now + " sleeping 60 sec");
		                    Thread.Sleep(60 * 1000);
						} catch {
							file.WriteLine("-- some exception " + DateTime.Now);
							Console.WriteLine("-- some exception " + DateTime.Now);
						}
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
