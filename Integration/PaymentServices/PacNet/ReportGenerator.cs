using System;
using System.IO;

namespace PaymentServices.PacNet
{
    class ReportGenerator
    {
        /**
         * <p>Parses and outputs the supplied string of Raven report data to the
         * console.</p>
         *
         * @param reportData the Raven report CSV data string
         * @throws IOException if the report data cannot be parsed and output
         */
        public void PrintReport(String reportData)
        {
            String[] reportLines = ParseReport(reportData);
            if (reportLines.Length > 0)
            {
                foreach (String reportLine in reportLines)
                {
                    Console.WriteLine(reportLine);
                }
            } 
            else 
            {
                Console.WriteLine("Report is empty.");
            }
        }

        /**
         * <p>Parses and outputs the supplied string of Raven report data to a file.</p>
         *
         * @param reportData the Raven report CSV data string
         * @param reportFileName the name of the report file
         * @throws IOException if the report data cannot be parsed and output
         */
        public void SaveReport(String reportData, String reportFileName)
        {

			//if (String.IsNullOrEmpty(reportFileName))
			//{

			//	SaveFileDialog fd = new SaveFileDialog();
			//	if (fd.ShowDialog() == DialogResult.OK)
			//	{
			//		reportFileName = fd.FileName;
			//	}
			//}

			//if (!String.IsNullOrEmpty(reportFileName))
			//{
			//	File.AppendAllLines(reportFileName, ParseReport(reportData));
			//}
        }

        /**
         * <p>Answers the supplied string of CSV data as an array of report lines.</p>
         *
         * @param reportData the string of Raven CSV data
         * @return an array of report lines
         */
        protected String[] ParseReport(String reportData)
        {
            if (String.IsNullOrEmpty(reportData))
            {
                return new String[] { };
            }
            else
            {
                return reportData.Split(new char[] { '\r' });
            }
        }
    }
}