SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_DecisionsSave') IS NOT NULL
	DROP PROCEDURE NL_DecisionsSave
GO

IF TYPE_ID('NL_DecisionsList') IS NOT NULL
	DROP TYPE NL_DecisionsList
GO

CREATE TYPE NL_DecisionsList AS TABLE (	
	[CashRequestID] [BIGINT] NOT NULL,
	[UserID] [INT] NOT NULL,
	[Position] [INT] NOT NULL,
	[DecisionTime] [DATETIME] NOT NULL,
	[DecisionNameID] [INT] NOT NULL,
	[Notes] [nvarchar](max) NULL
)
GO

CREATE PROCEDURE NL_DecisionsSave
@Tbl NL_DecisionsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Decisions (
		[CashRequestID],
		[UserID],
		[DecisionNameID],
		[DecisionTime],
		[Position],
		[Notes]
	) SELECT
		[CashRequestID],
		[UserID],
		[DecisionNameID],
		[DecisionTime],
		[Position],
		[Notes]		
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


