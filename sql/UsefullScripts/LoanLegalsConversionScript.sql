
IF EXISTS (SELECT * FROM LoanAgreementTemplateTypes WHERE TemplateType LIKE 'Alibaba%' )
	DROP TABLE LoanAgreementTemplateTypes
GO

IF object_id('LoanAgreementTemplateTypes') IS NULL
BEGIN
CREATE TABLE dbo.LoanAgreementTemplateTypes(
	TemplateTypeID int NOT NULL,
	TemplateType nvarchar(50) NOT NULL,
	TimestampCounter timestamp NOT NULL)
END
GO

IF NOT EXISTS (SELECT * FROM LoanAgreementTemplateTypes)
BEGIN
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (1,'GuarantyAgreement')
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (2,'PreContract')
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (3,'RegulatedLoanAgreement')
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (4,'PrivateCompanyLoanAgreement')
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (5,'CreditFacility')
	INSERT INTO LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES (6,'BoardResolution')
END
GO


-- Conversion script
ALTER TABLE LoanAgreementTemplate DROP  CONSTRAINT [DF_LoanAgreementTemplate_TemplateTypeID]
ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT [DF__LoanAgree__Templ__60283922]
ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT [DF__LoanAgree__IsUpd__7C2721CA]
ALTER TABLE LoanAgreementTemplate DROP COLUMN TemplateType 
ALTER TABLE LoanAgreementTemplate ADD OriginID INT
ALTER TABLE LoanAgreementTemplate ADD IsRegulated BIT
ALTER TABLE LoanAgreementTemplate ADD ProductID INT
ALTER TABLE LoanAgreementTemplate ADD IsApproved BIT
ALTER TABLE LoanAgreementTemplate ADD IsReviewed BIT
ALTER TABLE LoanAgreementTemplate ADD ReleaseDate DATETIME
GO


update LoanAgreementTemplate set ProductID = 1

update LoanAgreementTemplate set originId = 1
update LoanAgreementTemplate set originId = 2 where Template like '%everline%'
update LoanAgreementTemplate set originId = 3 where TemplateTypeID >= 5

update LoanAgreementTemplate set TemplateTypeID = 1 where template like '%THIS GUARANTEE is made and entered into on%' 
update LoanAgreementTemplate set TemplateTypeID = 2 where template like '%PRE-CONTRACT CREDIT INFORMATION%' 
update LoanAgreementTemplate set TemplateTypeID = 3 where template like '%Fixed Sum Loan Agreement Regulated by the Consumer Credit Act%'
update LoanAgreementTemplate set TemplateTypeID = 4 where template like '%THIS AGREEMENT is made between:%' 
update LoanAgreementTemplate set TemplateTypeID = 5 where template like '%to set out the terms and conditions on which the Creditor is prepared to make available to the Customer a credit facilit%' 

update LoanAgreementTemplate set IsRegulated = 1 where TemplateTypeID in (2,3)
update LoanAgreementTemplate set IsRegulated = 0 where TemplateTypeID in (1,4,5,6)

update LoanAgreementTemplate set IsReviewed = 1 
update LoanAgreementTemplate set IsApproved = 0

ALTER TABLE LoanLegal ADD SignedLegalDocs varchar(max)
ALTER TABLE NL_LoanLegals ADD SignedLegalDocs varchar(max)
GO
