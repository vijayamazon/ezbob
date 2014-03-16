IF OBJECT_ID('BrokerLeadCanFillWizard') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLeadCanFillWizard AS SELECT 1')
GO

ALTER PROCEDURE BrokerLeadCanFillWizard
@LeadID INT,
@LeadEmail NVARCHAR(255),
@ContactEmail NVARCHAR(255)
AS
BEGIN
	DECLARE @BrokerID INT
	DECLARE @ErrMsg NVARCHAR(1024) = ''
	DECLARE @CustomerID INT
	DECLARE @OutLeadID INT
	DECLARE @OutLeadEmail NVARCHAR(255)

	IF @ErrMsg = ''
	BEGIN
		IF @LeadID > 0 AND LTRIM(RTRIM(ISNULL(@LeadEmail, ''))) != ''
			SET @ErrMsg = 'Both lead id and lead email specified while there can be only one.'
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@BrokerID = BrokerID
		FROM
			Broker
		WHERE
			ContactEmail = @ContactEmail
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@OutLeadEmail = Email,
			@OutLeadID = BrokerLeadID
		FROM
			BrokerLeads
		WHERE
			BrokerID = @BrokerID
			AND (
				BrokerLeadID = @LeadID
				OR
				Email = @LeadEmail
			)

		SELECT
			@CustomerID = Id
		FROM
			Customer
		WHERE
			BrokerID = @BrokerID
			AND
			Name = @OutLeadEmail

		IF @OutLeadID IS NOT NULL
			SELECT
				@OutLeadID AS LeadID,
				@OutLeadEmail AS LeadEmail,
				@CustomerID AS CustomerID
	END
END
GO
