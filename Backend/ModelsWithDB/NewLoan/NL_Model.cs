namespace Ezbob.Backend.ModelsWithDB.NewLoan
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using ConfigManager;
    using Ezbob.Utils.Attributes;

    [DataContract]
    public class NL_Model : AStringable
    {

        public NL_Model(int customerID)
        {

            CustomerID = customerID;

            Offer = new NL_Offers();
            Loan = new NL_Loans();
            Agreements = new List<NLAgreementItem>();
            FundTransfer = new NL_FundTransfers();
            CalculatorImplementation = CurrentValues.Instance.DefaultLoanCalculator.Value;

        } // constructor

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public int? UserID { get; set; }

        [DataMember]
        public NL_FundTransfers FundTransfer { get; set; }

        [DataMember]
        public NL_Loans Loan { get; set; }

        [DataMember]
        [ExcludeFromToString]
        public List<NLAgreementItem> Agreements { get; set; }

        [DataMember]
        public decimal? BrokerComissions { get; set; }

        [DataMember]
        public NL_Offers Offer { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public decimal? APR { get; set; }

        // use default from configuration
        [DataMember]
        public string CalculatorImplementation { get; private set; } // AloanCalculator LegacyLoanCalculator/BankLikeLoanCalculator


     /*   public ALoanCalculator CalculatorInstance()
        {
            // set default
            ALoanCalculator calc = new LegacyLoanCalculator(this);

            try
            {
                Type t = Type.GetType(CalculatorImplementation);

                if (t != null)
                {

                    if (t == typeof(BankLikeLoanCalculator))
                        calc = new BankLikeLoanCalculator(this);

                    //default type from configurations
                    //ALoanCalculator calc = (ALoanCalculator)Activator.CreateInstance(t, this);
                    //if (t.GetType() == typeof(BankLikeLoanCalculator))
                    //     calc = (BankLikeLoanCalculator)Activator.CreateInstance(t, this); // new BankLikeLoanCalculator(this);
                    //else if (t.GetType() == typeof(LegacyLoanCalculator))
                    //     calc = (LegacyLoanCalculator)Activator.CreateInstance(t, this); // calc = new LegacyLoanCalculator(this);

                    Console.WriteLine(calc);

                    return calc;
                }
                // ReSharper disable once CatchAllClause
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // ReSharper disable once ThrowingSystemException
                throw new Exception(string.Format("Failed to create calculator instance for {0}", CalculatorImplementation), e);
            }
            return null;
        }*/




    } // class NL_Model
} // namespace