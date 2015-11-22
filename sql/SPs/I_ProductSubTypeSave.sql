SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ProductSubTypeSave') IS NOT NULL
	DROP PROCEDURE I_ProductSubTypeSave
GO

IF TYPE_ID('I_ProductSubTypeList') IS NOT NULL
	DROP TYPE I_ProductSubTypeList
GO

CREATE TYPE I_ProductSubTypeList AS TABLE (
	[ProductTypeID] INT NOT NULL,
	[ProductTermID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[AllowedForExternalInvestor] BIT NOT NULL,
	[PulledLoans] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_ProductSubTypeSave
@Tbl I_ProductSubTypeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_ProductSubType (
		[ProductTypeID],
		[ProductTermID],
		[Name],
		[AllowedForExternalInvestor],
		[PulledLoans],
		[Timestamp]
	) SELECT
		[ProductTypeID],
		[ProductTermID],
		[Name],
		[AllowedForExternalInvestor],
		[PulledLoans],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


