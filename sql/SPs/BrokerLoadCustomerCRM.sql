IF OBJECT_ID('BrokerLoadCustomerCRM') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerCRM AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerCRM
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

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
		c.RefNumber = @RefNum
		ANd
		c.BrokerID = @BrokerID
	ORDER BY
		cr.Timestamp DESC
END
GO
