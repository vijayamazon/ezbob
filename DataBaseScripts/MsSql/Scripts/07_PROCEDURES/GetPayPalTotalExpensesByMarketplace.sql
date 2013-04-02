﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPayPalTotalExpensesByMarketplace]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPayPalTotalExpensesByMarketplace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalExpensesByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT * from GetTotalExpensesPayPalTransactions (@marketplaceId)
END
GO
