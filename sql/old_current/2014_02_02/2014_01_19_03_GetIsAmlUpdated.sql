IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIsAmlUpdated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIsAmlUpdated]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIsAmlUpdated] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@AmlResult NVARCHAR(100)
		
	SELECT @AmlResult = AMLResult FROM Customer WHERE Id = @CustomerId 
	
	IF @AmlResult IS NOT NULL
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated	
END
GO