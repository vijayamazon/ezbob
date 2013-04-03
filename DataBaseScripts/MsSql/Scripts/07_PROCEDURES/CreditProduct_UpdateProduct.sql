IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_UpdateProduct]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_UpdateProduct]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_UpdateProduct]
  (
    @pId int OUTPUT,
    @pName nvarchar(max),
    @pDescription nvarchar(max),
    @pDateCreation datetime,
    @pUserId int,
    @psignedDocument ntext,
    @pdata ntext
   )
AS

BEGIN

  if @pId is not null 
	begin
		if exists (SELECT Id FROM creditproduct_products WHERE name=@pName AND id <> @pId AND IsDeleted is null)
		begin
			RAISERROR('IX_CREDITPRODUCT_NAME', 16, 1);
			return;
		end
		 update creditproduct_products
			set name = @pName,
				description = @pDescription,
				creationdate = @pDateCreation,
				userid = @pUserId
		  where id = @pId;
	end
  else
	begin
		if exists (SELECT Id FROM creditproduct_products WHERE name=@pName AND IsDeleted is null)
		begin
			RAISERROR('IX_CREDITPRODUCT_NAME', 16, 1);
			return;
		end
		insert into creditproduct_products
		  (name, description, creationdate, userid)
		values
		  (@pName, @pDescription, GETDATE(), @pUserId);
		SET @pId = @@IDENTITY;
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
END
GO
