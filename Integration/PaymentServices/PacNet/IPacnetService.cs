namespace PaymentServices.PacNet
{
    public interface IPacnetService
    {
        PacnetReturnData SendMoney(int userId, decimal amount, string bankNumber, string accountNumber, string accountName,
                                          string fileName = null, string currencyCode = "GBP", string description = null);

        PacnetReturnData CheckStatus(int userId, string trackingNumber);
        PacnetReturnData CloseFile(int userId, string fileName);
    }
}
