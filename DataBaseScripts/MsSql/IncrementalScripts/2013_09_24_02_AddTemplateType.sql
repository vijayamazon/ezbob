IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TemplateType' and Object_ID = Object_ID(N'LoanAgreementTemplate'))    
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD TemplateType INT DEFAULT 1 NOT NULL
END
GO

UPDATE LoanAgreementTemplate SET TemplateType = 2 WHERE Template LIKE '<center>%'
UPDATE LoanAgreementTemplate SET TemplateType = 3 WHERE Template LIKE '%Fixed Sum%' AND Template LIKE '<h4%'
UPDATE LoanAgreementTemplate SET TemplateType = 4 WHERE Template LIKE '%THIS IS AN IMPORTANT%'
GO
