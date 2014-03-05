IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByRange
GO

CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByRange]
(	@marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL(Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Transfer' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY pt.CustomerMarketPlaceId
)

GO

