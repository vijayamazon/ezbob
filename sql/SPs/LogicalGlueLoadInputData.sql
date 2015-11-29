SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadInputData') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInputData AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadInputData
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SELECT
		c.Id,
		c.CompanyID,
		c.FirstName,
		c.Surname,
		c.DateOfBirth
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
END
GO
