IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBiggestExpensesPayPalTransactions]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetBiggestExpensesPayPalTransactions]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetBiggestExpensesPayPalTransactions]
(	
  @marketpalceId int
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
