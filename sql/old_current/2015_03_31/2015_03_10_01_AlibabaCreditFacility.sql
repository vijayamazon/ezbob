SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanLegal') AND name = 'AlibabaCreditFacilityTemplateID')
BEGIN
	ALTER TABLE LoanLegal ADD AlibabaCreditFacilityTemplateID INT NULL
	ALTER TABLE LoanLegal ADD CONSTRAINT FK_LoanLegal_AlibabaCreditFacility FOREIGN KEY (AlibabaCreditFacilityTemplateID) REFERENCES LoanAgreementTemplate (Id)
END
GO
