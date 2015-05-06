SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_DecisionsSave') IS NOT NULL
	DROP PROCEDURE NL_DecisionsSave
GO

IF TYPE_ID('NL_DecisionsList') IS NOT NULL
	DROP TYPE NL_DecisionsList
GO

CREATE TYPE NL_DecisionsList AS TABLE (
	[CashRequestID] INT NOT NULL,
	[UserID] INT NOT NULL,
	[DecisionNameID] INT NOT NULL,
	[DecisionTime] DATETIME NOT NULL,
	[Position] INT NOT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[IsRepaymentPeriodSelectionAllowed] BIT NULL,
	[IsAmountSelectionAllowed] BIT NULL,
	[InterestOnlyRepaymentCount] INT NULL,
	[SendEmailNotification] BIT NULL
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
		[Notes],
		[IsRepaymentPeriodSelectionAllowed],
		[IsAmountSelectionAllowed],
		[InterestOnlyRepaymentCount],
		[SendEmailNotification]
	) SELECT
		[CashRequestID],
		[UserID],
		[DecisionNameID],
		[DecisionTime],
		[Position],
		[Notes],
		[IsRepaymentPeriodSelectionAllowed],
		[IsAmountSelectionAllowed],
		[InterestOnlyRepaymentCount],
		[SendEmailNotification]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


