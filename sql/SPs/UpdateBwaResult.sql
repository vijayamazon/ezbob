IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateBwaResult]') AND TYPE IN (N'P', N'PC'))
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
