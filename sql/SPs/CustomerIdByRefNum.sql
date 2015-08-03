IF OBJECT_ID('CustomerIdByRefNum') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerIdByRefNum AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CustomerIdByRefNum
@CustomerRefNum NVARCHAR(8)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT Id from Customer where [RefNumber] = @CustomerRefNum
	
END
GO


