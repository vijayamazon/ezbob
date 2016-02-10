IF OBJECT_ID('GetCustomerOrigin') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerOrigin AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerOrigin
@CustomerID INT
AS
BEGIN
	SELECT
		c.OriginID
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
END
GO
