IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCreditProductParams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCreditProductParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCreditProductParams]
(
	@iCreditProductName nvarchar(4000)
)
AS
BEGIN
    select
      cpp.id
      ,cpp.name
      ,cpp.description
      ,cpp.type
      ,cpp.value
	  ,cpp.creditproductid
    from
      creditproduct_products cp,
      creditproduct_params cpp
    where
      cp.id = cpp.creditproductid
      and cp.name = @iCreditProductName;
END;
GO
