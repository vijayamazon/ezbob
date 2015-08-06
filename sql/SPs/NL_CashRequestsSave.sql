SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_CashRequestsSave') IS NOT NULL
	DROP PROCEDURE NL_CashRequestsSave
GO

IF TYPE_ID('NL_CashRequestsList') IS NOT NULL
	DROP TYPE NL_CashRequestsList
GO

CREATE TYPE NL_CashRequestsList AS TABLE (
	[CustomerID] INT NOT NULL,
	[RequestTime] DATETIME NOT NULL,
	[CashRequestOriginID] INT NOT NULL,
	[UserID] INT NOT NULL,
	[OldCashRequestID] BIGINT NOT NULL
)
GO

CREATE PROCEDURE NL_CashRequestsSave
@Tbl NL_CashRequestsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_CashRequests (
		[CustomerID],
		[RequestTime],
		[CashRequestOriginID],
		[UserID],
		[OldCashRequestID]
	) SELECT
		[CustomerID],
		[RequestTime],
		[CashRequestOriginID],
		[UserID],
		[OldCashRequestID]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


