IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTransactionsCountPayPalTransactionsByRange]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTransactionsCountPayPalTransactionsByRange]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
19.10.2012 O.Zemskyi Поменял условие отбора транзакций с pi.NetAmountAmount < 0 на pi.NetAmountAmount > 0
*/
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
