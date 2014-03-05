IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_GetProductList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_GetProductList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_GetProductList]
AS
BEGIN
	select
     id
    ,name
    ,description
    ,creationdate
    ,userid
    ,(select TOP 1 scp.SignedDocument from CreditProduct_Sign scp where scp.CreditProductId = p.Id ORDER BY Id DESC) as SignedDocument
  from creditproduct_products p
  where isdeleted is null
END
GO
