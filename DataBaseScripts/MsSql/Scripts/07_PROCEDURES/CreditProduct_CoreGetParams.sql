IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_CoreGetParams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_CoreGetParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_CoreGetParams]
	(
		@pCreditProductName nvarchar(max)
	) 
AS

BEGIN

    select a.name, a.value
      from creditproduct_params a,
           Creditproduct_Products b
     where a.creditproductid = b.id and upper(b.Name) = upper(@pCreditProductName);

END
GO
