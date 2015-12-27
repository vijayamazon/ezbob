SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreateAlibabaBuyer') IS NULL
	EXECUTE('CREATE PROCEDURE CreateAlibabaBuyer AS SELECT 1')
GO

ALTER PROCEDURE CreateAlibabaBuyer
@CustomerID INT,
@AliID BIGINT
AS
BEGIN
	INSERT INTO AlibabaBuyer (AliId, CustomerId)
		VALUES (@AliID, @CustomerID)
END
GO
