SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_ParameterSave') IS NOT NULL
	DROP PROCEDURE I_ParameterSave
GO

IF TYPE_ID('I_ParameterList') IS NOT NULL
	DROP TYPE I_ParameterList
GO

CREATE TYPE I_ParameterList AS TABLE (
	[Name] NVARCHAR(255) NULL,
	[ValueType] NVARCHAR(255) NULL,
	[DefaultValue] DECIMAL(18, 6) NULL,
	[MaxLimit] DECIMAL(18, 6) NULL,
	[MinLimit] DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE I_ParameterSave
@Tbl I_ParameterList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Parameter (
		[Name],
		[ValueType],
		[DefaultValue],
		[MaxLimit],
		[MinLimit]
	) SELECT
		[Name],
		[ValueType],
		[DefaultValue],
		[MaxLimit],
		[MinLimit]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


