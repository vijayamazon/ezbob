DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfLoanPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfLoanPath1', 'C:\Temp\Agreements\nlpdf1', 'Agreement pdf loan path 1 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfLoanPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfLoanPath2', 'C:\Temp\Agreements\nlpdf2', 'Agreement pdf loan path 2 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfConsentPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfConsentPath1', 'C:\Temp\Agreements\nlconsent1', 'Agreement pdf consent path 1 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfConsentPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfConsentPath2', 'C:\Temp\Agreements\nlconsent2', 'Agreement pdf consent path 2 - new loan')
	END
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfLoanPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfLoanPath1', 'C:\inetpub\ezbob\Temp\TempStates\nlpdf1', 'Agreement pdf loan path 1 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfLoanPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfLoanPath2', 'C:\inetpub\ezbob\Temp\TempStates\nlpdf2', 'Agreement pdf loan path 2 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfConsentPath1')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfConsentPath1', 'C:\inetpub\ezbob\Temp\TempStates\nlconsent01', 'Agreement pdf consent path 1 - new loan')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NL_AgreementPdfConsentPath2')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NL_AgreementPdfConsentPath2', 'C:\inetpub\ezbob\Temp\TempStates\nlconsent02', 'Agreement pdf consent path 2 - new loan')
	END
END
GO
