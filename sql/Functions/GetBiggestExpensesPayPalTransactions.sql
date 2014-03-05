IF OBJECT_ID (N'dbo.GetBiggestExpensesPayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetBiggestExpensesPayPalTransactions
GO

CREATE FUNCTION [dbo].[GetBiggestExpensesPayPalTransactions]
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
			AND Payer != '' AND pi.NetAmountAmount < 0
		Group BY Payer) as tr
		ORDER BY tr.Income24Plus
)

GO

