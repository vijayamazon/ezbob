IF OBJECT_ID('BrokerAddCustomerLead') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerAddCustomerLead AS SELECT 1')
GO

ALTER PROCEDURE BrokerAddCustomerLead
@LeadFirstName NVARCHAR(250),
@LeadLastName NVARCHAR(250),
@LeadEmail NVARCHAR(128),
@LeadAddMode NVARCHAR(10),
@ContactEmail NVARCHAR(255),
@DateCreated DATETIME
AS
BEGIN
	DECLARE @ErrMsg NVARCHAR(1024) = ''
	DECLARE @BrokerID INT
	DECLARE @BrokerLeadAddModeID INT

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckContactInfoUniqueness(@LeadEmail, NULL, DEFAULT, DEFAULT, DEFAULT)
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT @BrokerID = BrokerID FROM Broker WHERE ContactEmail = @ContactEmail

		IF @BrokerID IS NULL
			SET @ErrMsg = 'No broker found with contact email ' + @ContactEmail
	END

	IF @ErrMsg = ''
	BEGIN
		SELECT @BrokerLeadAddModeID = BrokerLeadAddModeID FROM BrokerLeadAddModes WHERE UPPER(BrokerLeadAddModeCode) = UPPER(@LeadAddMode)

		IF @BrokerLeadAddModeID IS NULL
			SET @ErrMsg = 'Invalid lead add mode: ' + @LeadAddMode
	END

	IF @ErrMsg = ''
	BEGIN
		INSERT INTO BrokerLeads (BrokerID, CustomerID, FirstName, LastName, Email, DateCreated, BrokerLeadDeletedReasonID, DateDeleted, BrokerLeadAddModeID)
			VALUES (@BrokerID, NULL, @LeadFirstName, @LeadLastName, @LeadEmail, @DateCreated, NULL, NULL, @BrokerLeadAddModeID)
	END

	SELECT @ErrMsg AS ErrorMsg
END
GO
