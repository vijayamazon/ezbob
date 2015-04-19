SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCifasPlusCases') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCifasPlusCases
GO

IF TYPE_ID('CallCreditDataCifasPlusCasesList') IS NOT NULL
	DROP TYPE CallCreditDataCifasPlusCasesList
GO

CREATE TYPE CallCreditDataCifasPlusCasesList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[CaseId] INT NULL,
	[OwningMember] INT NULL,
	[ManagingMember] INT NULL,
	[CaseType] NVARCHAR(10) NULL,
	[ProductCode] NVARCHAR(10) NULL,
	[Facility] NVARCHAR(10) NULL,
	[SupplyDate] DATETIME NULL,
	[ExpiryDate] DATETIME NULL,
	[ApplicationDate] DATETIME NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCifasPlusCases
@Tbl CallCreditDataCifasPlusCasesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataCifasPlusCasesId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataCifasPlusCases table.', 11, 1)

	INSERT INTO CallCreditDataCifasPlusCases (
		[CallCreditDataID],
		[CaseId],
		[OwningMember],
		[ManagingMember],
		[CaseType],
		[ProductCode],
		[Facility],
		[SupplyDate],
		[ExpiryDate],
		[ApplicationDate]
	) SELECT
		[CallCreditDataID],
		[CaseId],
		[OwningMember],
		[ManagingMember],
		[CaseType],
		[ProductCode],
		[Facility],
		[SupplyDate],
		[ExpiryDate],
		[ApplicationDate]
	FROM @Tbl

	SET @CallCreditDataCifasPlusCasesId = SCOPE_IDENTITY()

	SELECT @CallCreditDataCifasPlusCasesId AS CallCreditDataCifasPlusCasesId
END
GO


