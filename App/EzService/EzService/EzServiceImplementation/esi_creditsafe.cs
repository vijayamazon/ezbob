namespace EzService.EzServiceImplementation
{
    using Ezbob.Backend.Strategies.CreditSafe;

    public partial class EzServiceImplementation
    {
        public ActionMetaData ParseCreditSafeLtd(int customerID, int userID, long serviceLogID) {
            return Execute<ParseCreditSafeLtd>(customerID, userID, serviceLogID);
        }//ParseCreditSafeLtd
        public ActionMetaData ParseCreditSafeNonLtd(int customerID, int userID, long serviceLogID)
        {
            return Execute<ParseCreditSafeNonLtd>(customerID, userID, serviceLogID);
        }//ParseCreditSafeNonLtd
    }//class EzServiceImplementation
}//ns
