IF OBJECT_ID (N'dbo.GetTransactionsCountPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetTransactionsCountPayPalTransactionsByRange
GO

CREATE FUNCTION [dbo].[GetTransactionsCountPayPalTransactionsByRange]
(	
@marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		Count(pi.NetAmountAmount) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY pt.CustomerMarketPlaceId
)

GO

