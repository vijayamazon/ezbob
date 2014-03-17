IF OBJECT_ID('BrokerDeleteCustomerLead') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDeleteCustomerLead AS SELECT 1')
GO

ALTER PROCEDURE BrokerDeleteCustomerLead
@CustomerID INT,
@ReasonCode NVARCHAR(10),
@DateDeleted DATETIME
AS
BEGIN
	DECLARE @ReasonID INT

	SELECT
		@ReasonID = BrokerLeadDeletedReasonID
	FROM
		BrokerLeadDeletedReasons
	WHERE
		BrokerLeadDeletedReasonCode = @ReasonCode

	IF @ReasonID IS NOT NULL
	BEGIN
		UPDATE BrokerLeads SET
			DateDeleted = @DateDeleted,
			BrokerLeadDeletedReasonID = @ReasonID
		WHERE
			CustomerID = @CustomerID
			AND
			DateDeleted IS NULL
			AND
			BrokerLeadDeletedReasonID IS NULL
	END
END
GO
