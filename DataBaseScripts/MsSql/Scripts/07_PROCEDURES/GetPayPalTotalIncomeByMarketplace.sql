IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalTotalIncomeByMarketplace]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalTotalIncomeByMarketplace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalIncomeByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT * from GetTotalIncomePayPalTransactions (@marketplaceId)
END
GO
