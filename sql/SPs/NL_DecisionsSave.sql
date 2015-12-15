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
	[DecisionNameID] [INT] NOT NULL,
	[DecisionTime] [DATETIME] NOT NULL,
	[Position] [INT] NOT NULL,
	[Notes] [nvarchar](max) NULL
)
GO

CREATE PROCEDURE NL_DecisionsSave
@Tbl NL_DecisionsList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @lastPosition int; 

	set @lastPosition = (select Max(Position) from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] cr on cr.CashRequestID = d.CashRequestID	
	and cr.CustomerID = (select CustomerID from [dbo].[NL_CashRequests] where CashRequestID = (select [CashRequestID] from @Tbl)))

	IF @lastPosition IS NULL
	BEGIN
		SET @lastPosition = 0
	END

	select @lastPosition = @lastPosition + 1;

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
		@lastPosition as [Position],
		[Notes]		
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


