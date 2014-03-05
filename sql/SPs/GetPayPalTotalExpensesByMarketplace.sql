IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalTotalExpensesByMarketplace]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalTotalExpensesByMarketplace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalExpensesByMarketplace] 
	(@marketplaceId INT)
AS
BEGIN
	SELECT NEWID() as Id, * from GetTotalExpensesPayPalTransactions (@marketplaceId)
END
GO
