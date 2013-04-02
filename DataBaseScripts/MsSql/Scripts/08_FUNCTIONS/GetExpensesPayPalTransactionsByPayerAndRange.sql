IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExpensesPayPalTransactionsByPayerAndRange]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetExpensesPayPalTransactionsByPayerAndRange]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByPayerAndRange]
(	
  @payer nvarchar(255),
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL( -Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pi.Payer = @payer AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount < 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY Payer
)
GO
