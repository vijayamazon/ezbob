namespace PaymentServices.PacNet
{
    public interface IPacnetService
    {
        PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName,
                                          string fileName = null, string currencyCode = "GBP", string description = null);

        PacnetReturnData CheckStatus(int customerId, string trackingNumber);
        PacnetReturnData CloseFile(int customerId, string fileName);
    }
}
