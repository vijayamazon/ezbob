IF OBJECT_ID('BrokerAddCustomerLead') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerAddCustomerLead AS SELECT 1')
GO

ALTER PROCEDURE BrokerAddCustomerLead
@LeadFirstName NVARCHAR(250),
@LeadLastName NVARCHAR(250),
@LeadEmail NVARCHAR(128),
@LeadAddMode NVARCHAR(10),
@ContactEmail NVARCHAR(255),
@DateCreated DATETIME,
@Origin INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(1024) = ''
	DECLARE @BrokerID INT
	DECLARE @BrokerLeadAddModeID INT
	DECLARE @SendEmail INT = 0
	DECLARE @LeadID INT = 0

	IF @ErrMsg = ''
	BEGIN
		SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail AND OriginID = @Origin

		IF @BrokerID IS NULL
			SET @ErrMsg = 'No broker found with contact email ' + @ContactEmail
	END

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckEmailUniqueness(@LeadEmail, @Origin, DEFAULT, DEFAULT, DEFAULT)
	END

	IF @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT BrokerID FROM Broker WHERE ContactEmail = @LeadEmail)
			SET @ErrMsg = 'email is already being used'
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT
			@BrokerLeadAddModeID = BrokerLeadAddModeID,
			@SendEmail = SendEmailOnCreate
		FROM
			BrokerLeadAddModes
		WHERE
			UPPER(BrokerLeadAddModeCode) = UPPER(@LeadAddMode)

		IF @BrokerLeadAddModeID IS NULL
			SET @ErrMsg = 'Invalid lead add mode: ' + @LeadAddMode
	END

	IF @ErrMsg = ''
	BEGIN
		INSERT INTO BrokerLeads (BrokerID, CustomerID, FirstName, LastName, Email, DateCreated, BrokerLeadDeletedReasonID, DateDeleted, BrokerLeadAddModeID)
			VALUES (@BrokerID, NULL, @LeadFirstName, @LeadLastName, @LeadEmail, @DateCreated, NULL, NULL, @BrokerLeadAddModeID)

		SET @LeadID = SCOPE_IDENTITY()
	END

	SELECT @ErrMsg AS ErrorMsg, @LeadID AS LeadID, @SendEmail AS SendEmailOnCreate
END
GO
