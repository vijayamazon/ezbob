IF OBJECT_ID('BrokerLoadCustomerCRM') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerCRM AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerCRM
@CustomerID INT,
@ContactEmail NVARCHAR(255)
AS
BEGIN
	DECLARE @BrokerID INT

	SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

	SELECT
		cr.Id,
		cr.Timestamp AS CrDate,
		a.Name AS ActionName,
		s.Name AS StatusName,
		cr.Comment
	FROM
		CustomerRelations cr
		INNER JOIN CRMActions a ON cr.ActionId = a.Id
		INNER JOIN CRMStatuses s ON cr.StatusId = s.Id
		INNER JOIN Customer c ON cr.CustomerId = c.Id
	WHERE
		cr.CustomerId = @CustomerID
		ANd
		c.BrokerID = @BrokerID
	ORDER BY
		cr.Timestamp DESC
END
GO
