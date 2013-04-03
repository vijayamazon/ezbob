IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_IsParamExists]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_IsParamExists]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_IsParamExists]
(
    @paramName nvarchar(max),
    @creditProductName nvarchar(max),
    @paramType nvarchar(max),
    @defaultValue nvarchar(max),
    @userId int
  )

AS
BEGIN
   DECLARE @l_id as int, @creditProductId as int, @prevType as nvarchar(max);
   
   select @creditProductId = id from creditproduct_products
       where upper(name) = upper(@creditProductName)
       and isdeleted is null;
	if  @creditProductId is null
	begin
        SELECT 3;
		RETURN 3; 
  end

    select @l_id = creditproduct_params.id, @prevType =  creditproduct_params.type
      from creditproduct_params
     where upper(creditproduct_params.name) = upper(@paramName)
	 and creditproduct_params.type = @paramType
           and creditproduct_params.creditproductid = @creditProductId;
     
  IF @l_id is not null
	BEGIN
		SELECT 0;
		RETURN 0; -- just update name
	END
  ELSE
	BEGIN
		SELECT 3;
		RETURN 3; -- create param
	END

END
GO
