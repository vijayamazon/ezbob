IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_DeleteProduct]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_DeleteProduct]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_DeleteProduct]
  (
    @pId int,
    @pUserId int,
    @psignedDocument ntext,
    @pdata ntext
   )
AS
  
BEGIN
  if @pId > 0 
	begin
		 update creditproduct_products
			set isdeleted = @pId
		  where id = @pId;
	end;

	if @psignedDocument is not null
	begin
		INSERT INTO [CreditProduct_Sign]
			   ([CreditProductId]
			   ,[Data]
			   ,[SignedDocument]
			   ,[UserId])
		VALUES
			   (@pId
			   ,@pdata
			   ,@psignedDocument
			   ,@puserId)
	end

END;
GO
