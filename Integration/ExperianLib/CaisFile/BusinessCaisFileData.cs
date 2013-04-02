using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class BusinessCaisFileData
    {
        public BusinessHeader Header { get; private set; }
        public List<BusinessAccountRecord> Accounts { get; private set; }
        public BusinessTrailer Trailer { get; private set; }

        public BusinessCaisFileData()
        {
            Header = new BusinessHeader();
            Accounts = new List<BusinessAccountRecord>();
            Trailer = new BusinessTrailer();
        }

        //-----------------------------------------------------------------------------------
        public void ReadFromFile(string filePath)
        {
            ReadFromString(File.ReadAllText(filePath));
        }

        //-----------------------------------------------------------------------------------
        public void ReadFromString(string data)
        {
            var lines = data.Split(new[]{"\r\n"},StringSplitOptions.RemoveEmptyEntries);
            if(lines.Length < 2) throw new Exception("Invalid input string, at least 3 lines expected");
            Header.Deserialize(lines[0]);
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var acc = new BusinessAccountRecord();
                acc.Deserialize(lines[i]);
                Accounts.Add(acc);
            }
            Trailer.Deserialize(lines[lines.Length - 1]);
        }

        //-----------------------------------------------------------------------------------
        public string WriteToString()
        {
            var str = new StringBuilder();
            str.AppendLine(Header.Serialize());
            foreach (var accountRecord in Accounts)
            {
                str.AppendLine(accountRecord.Serialize());
            }
            Trailer.TotalRecords = Accounts.Count;
            str.AppendLine(Trailer.Serialize());
            return str.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void WriteToFile(string fileName)
        {
            if(File.Exists(fileName))
            {
                var old = new BusinessCaisFileData();
                old.ReadFromFile(fileName);
                Accounts.AddRange(old.Accounts);
            }
            File.WriteAllText(fileName, WriteToString(), Encoding.ASCII);
        }
    }
}
