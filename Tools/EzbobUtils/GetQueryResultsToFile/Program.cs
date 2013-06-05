namespace GetQueryResultsToFile
{
    using DbConnection;
    using Logger;
    using System.Data;
    using System.Text;
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Logger.ErrorFormat("Invalid number of arguments. Expected:2 Actual:{0}", args.Length);
                Logger.ErrorFormat("Usage: GetQueryResultsToFile.exe \"<Sql to run>\" \"<Filename>\"", args.Length);
            }

            try
            {
                string sql = args[0];
                string filename = args[1];

                var sb = new StringBuilder();
                DataTable dt = DbConnection.ExecuteReader(sql);

                foreach (DataRow row in dt.Rows)
                {
                    bool firstInRow = true;
                    foreach (var fieldValue in row.ItemArray)
                    {
                        if (!firstInRow)
                        {
                            sb.Append(",");
                        }
                        sb.Append(fieldValue);
                        firstInRow = false;
                    }
                    sb.Append("\n");
                }

                File.WriteAllLines(filename, new[] { sb.ToString() });
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error creating result:{0}", e);
            }
        }
    }
}
