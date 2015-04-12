IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CHK_WSS_StepName')
BEGIN
	ALTER TABLE WizardStepSequence DROP CONSTRAINT CHK_WSS_StepName
END
GO

IF NOT EXISTS (SELECT * FROM WizardStepTypes WHERE WizardStepTypeName = 'companydetails')
BEGIN
	INSERT INTO WizardStepTypes (WizardStepTypeID, TheLastOne, WizardStepTypeName, WizardStepTypeDescription) VALUES
		(6, 0, 'companydetails', 'Company details (type, name, address, etc)')

	INSERT INTO WizardStepSequence (ID, StepName, OnlineProgressBarPct, OfflineProgressBarPct, WizardStepType) VALUES
		(3, 'companydetails', 90, 50, 6)
END
GO

UPDATE WizardStepTypes SET WizardStepTypeDescription = 'Personal details (name, address, phone number)' WHERE WizardStepTypeName = 'details'
GO
