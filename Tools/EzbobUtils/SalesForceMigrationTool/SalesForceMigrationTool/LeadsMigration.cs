using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace SalesForceMigrationTool {
    public class LeadsMigration {
        public List<LeadModel> ParseCsv(byte[] content, string fileName) {
            
            Stream stream = new MemoryStream(content);
            var csvData = new DataTable();

            using (var csvReader = new TextFieldParser(stream) {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true
            }) {
                csvReader.SetDelimiters(",");
                
                string[] colFields = csvReader.ReadFields();
                foreach (string column in colFields) {
                    var dataColumn = new DataColumn(column) {
                        AllowDBNull = true
                    };
                    csvData.Columns.Add(dataColumn);
                }
              
                while (!csvReader.EndOfData) {
                    string[] fieldData = csvReader.ReadFields();
                    //Making empty value as null
                    if (fieldData != null) {
                        for (int i = 0; i < fieldData.Length; i++) {
                            if (fieldData[i] == "")
                                fieldData[i] = null;
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }

            return BuildLeadsModel(csvData);
        }

        private List<LeadModel> BuildLeadsModel(DataTable csvData)
        {
            var leads = new List<LeadModel>();

            foreach (DataRow row in csvData.Rows) {
                try {
                    var lead = new LeadModel();
                    lead.Email = row[0].ToString();
                    lead.FirstName = row[1].ToString();
                    lead.Surname = row[2].ToString();
                    lead.MobilePhone = row[3].ToString();
                    lead.HomePhone = row[4].ToString();
                    lead.GroupTag = row[5].ToString();

                    leads.Add(lead);
                } catch (Exception ex) {
                    //log
                }
            }

          
            return leads;
        }
    }
}
