IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[creditproduct_getproductbyname]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[creditproduct_getproductbyname]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[creditproduct_getproductbyname]
(
  @pName nvarchar(max)
) 
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
  where isdeleted is null and name = @pName;
END
GO
