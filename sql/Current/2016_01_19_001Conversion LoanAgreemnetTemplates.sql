IF EXISTS (SELECT * FROM LoanAgreementTemplateTypes)
	DROP TABLE LoanAgreementTemplateTypes
GO

IF object_id('LoanAgreementTemplateTypes') IS NULL
BEGIN
CREATE TABLE LoanAgreementTemplateTypes(
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
IF OBJECT_ID('DF_LoanAgreementTemplate_TemplateTypeID', 'C') IS NOT NULL 
	ALTER TABLE LoanAgreementTemplate DROP  CONSTRAINT [DF_LoanAgreementTemplate_TemplateTypeID]
GO
IF OBJECT_ID('DF__LoanAgree__Templ__60283922', 'C') IS NOT NULL 
	ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT [DF__LoanAgree__Templ__60283922]
GO
IF OBJECT_ID('DF__LoanAgree__IsUpd__7C2721CA', 'C') IS NOT NULL 
	ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT [DF__LoanAgree__IsUpd__7C2721CA]
GO
IF COL_LENGTH('LoanAgreementTemplate','TemplateType') IS NOT NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate DROP COLUMN TemplateType 	
END
GO
IF COL_LENGTH('LoanAgreementTemplate','OriginID') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD OriginID INT
END
GO
IF COL_LENGTH('LoanAgreementTemplate','IsRegulated') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD IsRegulated BIT	
END
GO
IF COL_LENGTH('LoanAgreementTemplate','ProductID') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD ProductID INT
END
GO
IF COL_LENGTH('LoanAgreementTemplate','IsApproved') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD IsApproved BIT
END
GO
IF COL_LENGTH('LoanAgreementTemplate','IsReviewed') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD IsReviewed BIT
END
GO
IF COL_LENGTH('LoanAgreementTemplate','ReleaseDate') IS NULL
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD ReleaseDate DATETIME
END
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

IF NOT EXISTS (select * from LoanAgreementTemplate where IsReviewed = 1 and IsApproved = 1)
BEGIN
	update LoanAgreementTemplate set IsReviewed = 1 
	update LoanAgreementTemplate set IsApproved = 0
END

IF COL_LENGTH('LoanLegal','SignedLegalDocs') IS NULL
BEGIN
	ALTER TABLE LoanLegal ADD SignedLegalDocs varchar(max)
END
GO
IF COL_LENGTH('NL_LoanLegals','SignedLegalDocs') IS NULL
BEGIN
	ALTER TABLE NL_LoanLegals ADD SignedLegalDocs varchar(max)
END
GO