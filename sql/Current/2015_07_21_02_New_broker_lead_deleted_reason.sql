SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM BrokerLeadDeletedReasons WHERE BrokerLeadDeletedReasonCode = 'FINISHWZRD')
BEGIN
	INSERT INTO BrokerLeadDeletedReasons (BrokerLeadDeletedReasonCode, BrokerLeadDeletedReason)
		VALUES ('FINISHWZRD', 'UW has completed the wizard for customer')
END
GO
