IF OBJECT_ID('LoadCustomerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerStatus
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id AS CustomerID,
		c.GreetingMailSentDate AS SetDate,
		s.Name AS Status,
		s.IsDefault
	FROM
		Customer c
		INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
	WHERE
		(@CustomerID IS NULL OR c.Id = @CustomerID)
	ORDER BY
		c.Id
END
GO
