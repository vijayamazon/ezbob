DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfLoanPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfLoanPath1', 'C:\Temp\Agreements\pdf1', 'Agreement pdf loan path 1')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfLoanPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfLoanPath2', 'C:\Temp\Agreements\pdf2', 'Agreement pdf loan path 2')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfConsentPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfConsentPath1', 'C:\Temp\Agreements\consent1', 'Agreement pdf consent path 1')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfConsentPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfConsentPath2', 'C:\Temp\Agreements\consent2', 'Agreement pdf consent path 2')
	END
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfLoanPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfLoanPath1', 'C:\inetpub\ezbob\Temp\TempStates\pdf1', 'Agreement pdf loan path 1')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfLoanPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfLoanPath2', 'C:\inetpub\ezbob\Temp\TempStates\pdf2', 'Agreement pdf loan path 2')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfConsentPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfConsentPath1', 'C:\inetpub\ezbob\Temp\TempStates\consent01', 'Agreement pdf consent path 1')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AgreementPdfConsentPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AgreementPdfConsentPath2', 'C:\inetpub\ezbob\Temp\TempStates\consent02', 'Agreement pdf consent path 2')
	END
END
GO
