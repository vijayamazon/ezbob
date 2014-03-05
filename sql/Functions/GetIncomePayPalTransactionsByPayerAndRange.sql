IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByPayerAndRange') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByPayerAndRange
GO

CREATE FUNCTION [dbo].[GetIncomePayPalTransactionsByPayerAndRange]
(	@payer nvarchar(255),
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL( Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pi.Payer = @payer AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY Payer
)

GO

