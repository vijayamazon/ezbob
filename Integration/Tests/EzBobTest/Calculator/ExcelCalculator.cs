namespace EzBobTest.Calculator {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Ezbob.Backend.CalculateLoan.LoanCalculator;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using FluentNHibernate.Testing.Values;
    using OfficeOpenXml;

    public class ExcelCalculator : ILoanCalculator {
        public ExcelCalculator(NL_Model nlModel) {
            NL_Model = nlModel;



            using (ExcelPackage = new ExcelPackage(new FileInfo("D:\\Excel\\Calculator.xlsx"))) {

                Worksheet = ExcelPackage.Workbook.Worksheets[1];

                //Loan Amount:
                Worksheet.Cells["D5"].Value = nlModel.Loan.FirstHistory()
                    .Amount.ToString(CultureInfo.InvariantCulture);

                //Annual Interest Rate:
                Worksheet.Cells["D6"].Value = nlModel.Loan.LastHistory()
                    .InterestRate.ToString(CultureInfo.InvariantCulture);

                //Loan Length in Years
                Worksheet.Cells["D7"].Value = 1;

                //Number of Payments per Year:
                Worksheet.Cells["D8"].Value = 12;

                //Start Date of Loan:
                Worksheet.Cells["D9"].Value = "01-02-2016";

                //Optional Extra Per Month Payment:
                Worksheet.Cells["D10"].Value = "10";

                //var calcOptions = new ExcelCalculationOption() {
                //    AllowCirculareReferences = true
                //};

                //Worksheet.Calculate(calcOptions);


                var calcOptions = new ExcelCalculationOption() {
                    AllowCirculareReferences = true
                };

                Worksheet.Cells[5, 9].Formula ="=IF(IF(D5*D6*D7*D9>0,1,0),-PMT(D6/D8,D7*D8,D5),\"\")";

                Worksheet.Cells[5, 9].Calculate();

                ExcelPackage.Save();
            }

            ExcelPackage.Dispose();
            Worksheet.Dispose();

            ExcelPackage = new ExcelPackage(new FileInfo("D:\\Excel\\Calculator.xlsx"));
            Worksheet = ExcelPackage.Workbook.Worksheets[1];
        }

        private ExcelPackage ExcelPackage { get; set; }
        private ExcelWorksheet Worksheet { get; set; }

        public NL_Model NL_Model { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int RepaymentCount { get; set; }
        public DateTime StratDate { get; set; }
        public List<DateTime, decimal> Payments { get; set; }
        public List<NL_LoanSchedules> Scheduleses { get; set; }


        public decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null) {
            return 1;
        }

        public void CreateSchedule() {
            Scheduleses = new List<NL_LoanSchedules>();
            NL_LoanSchedules loanSchedule = new NL_LoanSchedules();



            
            
            var row = 14;

            while (!string.IsNullOrEmpty(Worksheet.Cells[row, 1].Text)) {
                //"Payment Number
                if (!string.IsNullOrEmpty(Worksheet.Cells[row, 1].Text))
                    loanSchedule.Position = Convert.ToInt32(Worksheet.Cells[row, 1].Text);

                //Payment Date
                if (!string.IsNullOrEmpty(Worksheet.Cells[row, 2].Text))
                    loanSchedule.PlannedDate = DateTime.ParseExact(Worksheet.Cells[row, 2].Text,
                        new[] {
                            "MM/dd/yyyy", "MM/dd/yy"
                        },
                        new CultureInfo("en-US"),
                        DateTimeStyles.None);

                //Begining Balance
                if (!string.IsNullOrEmpty(Worksheet.Cells[row, 3].Text)) {
                    var beginingBalance = Worksheet.Cells[row, 3].Text;
                }

                //Scheduale Payment
                if (!string.IsNullOrEmpty(Worksheet.Cells[row, 4].Text)) {
                    var schedualePayment = Worksheet.Cells[row, 4].Text;
                }

                //Extra Payment
                //loanSchedule.FeesPaid = Convert.ToDecimal(Worksheet.Cells[row, 5].Text);

                //TotalPayment 
                if ((!string.IsNullOrEmpty(Worksheet.Cells[row, 6].Text) && !Worksheet.Cells[row, 6].Text.Contains("$-")))
                    loanSchedule.AmountDue = Convert.ToDecimal(Worksheet.Cells[row, 6].Text.Replace("$", String.Empty));

                //Principal
                if ((!string.IsNullOrEmpty(Worksheet.Cells[row, 7].Text) && !Worksheet.Cells[row, 7].Text.Contains("$-")))
                    loanSchedule.PrincipalPaid = Convert.ToDecimal(Worksheet.Cells[row, 7].Text.Replace("$", String.Empty));

                //Interest
                if ((!string.IsNullOrEmpty(Worksheet.Cells[row, 8].Text) && !Worksheet.Cells[row, 8].Text.Contains("$-")))
                    loanSchedule.InterestPaid = Convert.ToDecimal(Worksheet.Cells[row, 8].Text.Replace("$", String.Empty));

                //Ending Balance
                if ((!string.IsNullOrEmpty(Worksheet.Cells[row, 9].Text) && !Worksheet.Cells[row, 9].Text.Contains("$-"))) {
                    var endingBalance = Worksheet.Cells[row, 9].Text.Replace("$", String.Empty);
                }


                //Cumulative Interest
                if ((!string.IsNullOrEmpty(Worksheet.Cells[row, 10].Text) && !Worksheet.Cells[row, 10].Text.Contains("$-")))
                    loanSchedule.InterestOP = Convert.ToDecimal(Worksheet.Cells[row, 10].Text.Replace("$", String.Empty));


                Scheduleses.Add(loanSchedule);

                row++;
            }

            //try {
            //    var calcOptions = new ExcelCalculationOption() {
            //        AllowCirculareReferences = true
            //    };

            //    Worksheet.Cells[row, 1].Calculate(calcOptions);
            //    Worksheet.Cells[row, 2].Calculate(calcOptions);
            //    Worksheet.Cells[row, 3].Calculate(calcOptions);
            //    Worksheet.Cells[row, 4].Calculate(calcOptions);
            //    Worksheet.Cells[row, 5].Calculate(calcOptions);
            //    Worksheet.Cells[row, 6].Calculate(calcOptions);
            //    Worksheet.Cells[row, 7].Calculate(calcOptions);
            //    Worksheet.Cells[row, 8].Calculate(calcOptions);
            //    Worksheet.Cells[row, 9].Calculate(calcOptions);
            //    Worksheet.Cells[row, 10].Calculate(calcOptions);
            //}
            //catch (Exception) {

            //}


        }


        public double CalculateApr(DateTime? aprDate = null, double? calculationAccuracy = null, ulong? maxIterationCount = null) {
            return 1;
        }

        public void GetState() { }
    }
}