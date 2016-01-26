SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CompaniesHouseOfficerAppointmentOrderSave') IS NOT NULL
	DROP PROCEDURE CompaniesHouseOfficerAppointmentOrderSave
GO

IF TYPE_ID('CompaniesHouseOfficerAppointmentOrderList') IS NOT NULL
	DROP TYPE CompaniesHouseOfficerAppointmentOrderList
GO

CREATE TYPE CompaniesHouseOfficerAppointmentOrderList AS TABLE (
	[CompaniesHouseOfficerOrderItemID] INT NOT NULL,
	[DobDay] INT NULL,
	[DobMonth] INT NULL,
	[DobYear] INT NULL,
	[Etag] NVARCHAR(255) NULL,
	[IsCorporateOfficer] BIT NOT NULL,
	[ItemsPerPage] INT NOT NULL,
	[Kind] NVARCHAR(255) NULL,
	[Link] NVARCHAR(255) NULL,
	[Name] NVARCHAR(255) NULL,
	[StartIndex] INT NOT NULL,
	[TotalResults] INT NOT NULL
)
GO

CREATE PROCEDURE CompaniesHouseOfficerAppointmentOrderSave
@Tbl CompaniesHouseOfficerAppointmentOrderList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CompaniesHouseOfficerAppointmentOrder (
		[CompaniesHouseOfficerOrderItemID],
		[DobDay],
		[DobMonth],
		[DobYear],
		[Etag],
		[IsCorporateOfficer],
		[ItemsPerPage],
		[Kind],
		[Link],
		[Name],
		[StartIndex],
		[TotalResults]
	) SELECT
		[CompaniesHouseOfficerOrderItemID],
		[DobDay],
		[DobMonth],
		[DobYear],
		[Etag],
		[IsCorporateOfficer],
		[ItemsPerPage],
		[Kind],
		[Link],
		[Name],
		[StartIndex],
		[TotalResults]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


