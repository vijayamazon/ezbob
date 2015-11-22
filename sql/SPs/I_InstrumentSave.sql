SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InstrumentSave') IS NOT NULL
	DROP PROCEDURE I_InstrumentSave
GO

IF TYPE_ID('I_InstrumentList') IS NOT NULL
	DROP TYPE I_InstrumentList
GO

CREATE TYPE I_InstrumentList AS TABLE (
	[Name] INT NOT NULL,
	[CurrencyID] INT NOT NULL
)
GO

CREATE PROCEDURE I_InstrumentSave
@Tbl I_InstrumentList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Instrument (
		[Name],
		[CurrencyID]
	) SELECT
		[Name],
		[CurrencyID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


