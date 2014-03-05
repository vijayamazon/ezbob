IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_GetLinkedStr]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_GetLinkedStr]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_GetLinkedStr] 
	(@pCreditProductId int)
AS
BEGIN
	select creditproduct_products.id,
           creditproduct_products.name as "ProductName",
           strategy_strategy.displayname as "Name",
           strategy_strategy.TermDate 
      from strategy_strategy,
           creditproduct_strategyrel,
           creditproduct_products
     where creditproductid = @pCreditProductId
           and strategy_strategy.strategyid = creditproduct_strategyrel.strategyid
           and creditproduct_products.id = creditproduct_strategyrel.creditproductid
		   and strategy_strategy.isdeleted = 0
END
GO
