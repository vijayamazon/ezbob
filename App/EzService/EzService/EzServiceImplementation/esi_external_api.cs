namespace EzService.EzServiceImplementation
{
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Backend.Strategies.ExternalAPI;
    using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;

    partial class EzServiceImplementation
    {
        public AlibabaAvailableCreditActionResult CustomerAvaliableCredit(string customerRefNum, long aliMemberID)
        {
            CustomerAvaliableCredit instance;

            Log.Info("ESI CustomerAvaliableCredit: customerID: {0}, customerID: {1}", customerRefNum, aliMemberID);

            ExecuteSync(out instance, null, null, customerRefNum, aliMemberID);

            return new AlibabaAvailableCreditActionResult { Result = instance.Result };

        } // CustomerAvaliableCredit

        public ActionMetaData RequalifyCustomer(string customerRefNum, long aliMemberID)
        {
            Log.Info("ESI RequalifyCustomer: customerID: {0}, customerID: {1}", customerRefNum, aliMemberID);

            ActionMetaData amd = Execute<RequalifyCustomer>(null, null, customerRefNum, aliMemberID);

            return amd;

        } //RequalifyCustomer

        public ActionMetaData SaveApiCall(ApiCallData data)
        {
            ActionMetaData amd = Execute<SaveApiCall>(data.CustomerID, null, data);

            return amd;

        } // SaveApiCall

        public AlibabaSaleContractActionResult SaleContract(AlibabaContractDto dto)
        {
            SaleContract instance;

            ExecuteSync(out instance, null, null, dto);

            return new AlibabaSaleContractActionResult { Result = instance.Result };

        } // SaveApiCall
    } // class EzServiceImplementation
} // namespace