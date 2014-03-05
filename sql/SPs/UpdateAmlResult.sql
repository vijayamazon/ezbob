IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAmlResult]') AND TYPE IN (N'P', N'PC'))
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
