IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalTotalTransactionsByMarketplace]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalTotalTransactionsByMarketplace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalTransactionsByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT NEWID() as Id, * from GetTotalTransactionsPayPalTransactions (@marketplaceId)
END
GO
