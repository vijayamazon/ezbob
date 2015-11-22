SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InterestVariableSave') IS NOT NULL
	DROP PROCEDURE I_InterestVariableSave
GO

IF TYPE_ID('I_InterestVariableList') IS NOT NULL
	DROP TYPE I_InterestVariableList
GO

CREATE TYPE I_InterestVariableList AS TABLE (
	[InstrumentID] INT NOT NULL,
	[TradeDate] DATETIME NOT NULL,
	[OneDay] DECIMAL(18, 6) NOT NULL,
	[OneWeek] DECIMAL(18, 6) NOT NULL,
	[OneMonth] DECIMAL(18, 6) NOT NULL,
	[TwoMonths] DECIMAL(18, 6) NOT NULL,
	[ThreeMonths] DECIMAL(18, 6) NOT NULL,
	[SixMonths] DECIMAL(18, 6) NOT NULL,
	[TwelveMonths] DECIMAL(18, 6) NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InterestVariableSave
@Tbl I_InterestVariableList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InterestVariable (
		[InstrumentID],
		[TradeDate],
		[OneDay],
		[OneWeek],
		[OneMonth],
		[TwoMonths],
		[ThreeMonths],
		[SixMonths],
		[TwelveMonths],
		[Timestamp]
	) SELECT
		[InstrumentID],
		[TradeDate],
		[OneDay],
		[OneWeek],
		[OneMonth],
		[TwoMonths],
		[ThreeMonths],
		[SixMonths],
		[TwelveMonths],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


