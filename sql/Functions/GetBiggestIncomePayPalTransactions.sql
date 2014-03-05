IF OBJECT_ID (N'dbo.GetBiggestIncomePayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetBiggestIncomePayPalTransactions
GO

CREATE FUNCTION [dbo].[GetBiggestIncomePayPalTransactions]
(	@marketpalceId int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT TOP 5 *
	FROM
	(SELECT 
		Payer,
		Sum(pi.NetAmountAmount) Income24Plus
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketpalceId
			AND Payer != '' AND pi.NetAmountAmount > 0
		Group BY Payer) as tr
		ORDER BY tr.Income24Plus DESC
)

GO

