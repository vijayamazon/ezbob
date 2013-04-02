IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_UpdateParams]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_UpdateParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_UpdateParams]
  (
    @pId int OUTPUT,
    @pName nvarchar(max),
    @pType nvarchar(max),
    @pDescription nvarchar(max),
    @pCreditProductId int,
    @pValue nvarchar(max)
   )
AS
  
BEGIN
	if @pId is not null 
	 BEGIN
		update creditproduct_params
		   set [name] = @pName,
			   [type] = @pType,
			   [description] = @pDescription,
			   creditproductid = @pCreditProductId,
			   [value] = @pValue
		 where id = @pId;
	 END

	  ELSE
		BEGIN
			insert into creditproduct_params
			  ([name], [type], [description], creditproductid, [value])
			values
			  (@pName, @pType, @pDescription, @pCreditProductId, @pValue);
			SET @pId = @@IDENTITY;
		END
END
GO
