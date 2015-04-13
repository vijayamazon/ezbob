SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasPlusCasesSubjects') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasPlusCasesSubjects
GO

IF TYPE_ID('CallCreditDataCifasPlusCasesSubjectsList') IS NOT NULL
	DROP TYPE CallCreditDataCifasPlusCasesSubjectsList
GO

CREATE TYPE CallCreditDataCifasPlusCasesSubjectsList AS TABLE (
	[CallCreditDataCifasPlusCasesID] BIGINT NULL,
	[PersonName] NVARCHAR(164) NULL,
	[PersonDob] DATETIME NULL,
	[CompanyName] NVARCHAR(70) NULL,
	[CompanyNumber] NVARCHAR(8) NULL,
	[HomeTelephone] NVARCHAR(20) NULL,
	[MobileTelephone] NVARCHAR(20) NULL,
	[Email] NVARCHAR(60) NULL,
	[SubjectRole] NVARCHAR(10) NULL,
	[SubjectRoleQualifier] NVARCHAR(10) NULL,
	[AddressType] NVARCHAR(10) NULL,
	[CurrentAddress] BIT NULL,
	[UndeclaredAddressType] INT NULL,
	[AddressValue] NVARCHAR(440) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasPlusCasesSubjects
@Tbl CallCreditDataCifasPlusCasesSubjectsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataCifasPlusCasesSubjects (
		[CallCreditDataCifasPlusCasesID],
		[PersonName],
		[PersonDob],
		[CompanyName],
		[CompanyNumber],
		[HomeTelephone],
		[MobileTelephone],
		[Email],
		[SubjectRole],
		[SubjectRoleQualifier],
		[AddressType],
		[CurrentAddress],
		[UndeclaredAddressType],
		[AddressValue]
	) SELECT
		[CallCreditDataCifasPlusCasesID],
		[PersonName],
		[PersonDob],
		[CompanyName],
		[CompanyNumber],
		[HomeTelephone],
		[MobileTelephone],
		[Email],
		[SubjectRole],
		[SubjectRoleQualifier],
		[AddressType],
		[CurrentAddress],
		[UndeclaredAddressType],
		[AddressValue]
	FROM @Tbl
END
GO


