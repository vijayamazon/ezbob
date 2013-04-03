IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_GetParams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_GetParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_GetParams]
(
  @pCreditProductId int

) 
AS

BEGIN


    select id, name, type, description, creditproductid, value from creditproduct_params
    where creditproductid = @pCreditProductId;


END;
GO
