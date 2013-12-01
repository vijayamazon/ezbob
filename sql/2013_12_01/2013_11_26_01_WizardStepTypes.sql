IF OBJECT_ID('WizardStepTypes') IS NULL
BEGIN
	CREATE TABLE WizardStepTypes (
		WizardStepTypeID INT NOT NULL,
		TheLastOne BIT NOT NULL,
		WizardStepTypeName NVARCHAR(64) NOT NULL,
		WizardStepTypeDescription NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_WizardStepTypes PRIMARY KEY (WizardStepTypeID),
		CONSTRAINT UC_WizardStepTypeName UNIQUE (WizardStepTypeName),
		CONSTRAINT CHK_WizardStepTypes CHECK (WizardStepTypeName != '')
	)

	INSERT INTO WizardStepTypes (WizardStepTypeID, TheLastOne, WizardStepTypeName, WizardStepTypeDescription) VALUES
		(1, 0, 'signup', 'Sign up to application'),
		(2, 0, 'link', 'Link shop/bank accounts (eBay, Amazon, Volusion, HMRC, Yodlee, etc)'),
		(3, 0, 'paymentlink', 'Link payment accounts (bank, Pay Pal) - not in use'),
		(4, 1, 'success', 'Wizard complete - thank you page'),
		(5, 0, 'details', 'Personal/company details')

	ALTER TABLE Customer ADD CONSTRAINT FK_Customer_WizardStepType FOREIGN KEY (WizardStep) REFERENCES WizardStepTypes(WizardStepTypeID)
	
	ALTER TABLE WizardStepSequence ADD CONSTRAINT FK_WizStepSeq_WizardStepType FOREIGN KEY (WizardStepType) REFERENCES WizardStepTypes(WizardStepTypeID)
END
GO
