IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAmlResult]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAmlResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateAmlResult] 
	(@CustomerId INT,
	 @AmlResult NVARCHAR(100))
AS
BEGIN
	UPDATE 
		Customer
	SET 
		AMLResult = @AmlResult
	WHERE
		Id = @CustomerId
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateBwaResult]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateBwaResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateBwaResult] 
	(@CustomerId INT,
	 @BwaResult NVARCHAR(100))
AS
BEGIN
	UPDATE 
		Customer
	SET 
		BWAResult = @BwaResult
	WHERE
		Id = @CustomerId
		
	UPDATE 
		CardInfo
	SET
		BWAResult = @BWAResult
	WHERE 
		CustomerId = @CustomerId
END
GO


