IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_AssignProduct]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_AssignProduct]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_AssignProduct] 
	(@pCreditProdName nvarchar(max),
    @pStrategyId int)
AS
BEGIN
	DECLARE @newId as int;
	DECLARE @pCreditProdId as int ;

    select @pCreditProdId = id  
    from creditproduct_products a
    where a.name = @pCreditProdName
    and a.isdeleted is null;

    select @newId = tbl.id from creditproduct_strategyrel tbl
    where tbl.creditproductid = @pCreditProdId and tbl.strategyid = @pStrategyId;
	
	IF @newId is null
	BEGIN
          insert into creditproduct_strategyrel
            (creditproductid, strategyid)
          values
            (@pCreditProdId, @pStrategyId);
	END
END
GO
