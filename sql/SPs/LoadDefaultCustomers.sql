IF OBJECT_ID('LoadDefaultCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDefaultCustomers AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadDefaultCustomers
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID
	FROM
		Customer c
		INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
	WHERE
		s.IsDefault = 1
END
GO
