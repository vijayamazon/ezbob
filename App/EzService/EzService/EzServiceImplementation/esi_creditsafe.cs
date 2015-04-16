namespace EzService.EzServiceImplementation
{
    using Ezbob.Backend.Strategies.CreditSafe;

    public partial class EzServiceImplementation
    {
        public ActionMetaData ParseCreditSafeLtd(int customerID, int userID, long serviceLogID) {
            return Execute<ParseCreditSafeLtd>(customerID, userID, serviceLogID);
        }//ParseCreditSafeLtd
    }//class EzServiceImplementation
}//ns
