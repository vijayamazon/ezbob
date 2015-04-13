SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataAccsHistory') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataAccsHistory
GO

IF TYPE_ID('CallCreditDataAccsHistoryList') IS NOT NULL
	DROP TYPE CallCreditDataAccsHistoryList
GO

CREATE TYPE CallCreditDataAccsHistoryList AS TABLE (
	[CallCreditDataAccsID] BIGINT NULL,
	[M] DATETIME NULL,
	[Bal] INT NULL,
	[CreditLimit] INT NULL,
	[Acc] NVARCHAR(10) NULL,
	[Pay] NVARCHAR(10) NULL,
	[StmtBal] INT NULL,
	[PayAmt] INT NULL,
	[CashAdvCount] INT NULL,
	[CashAdvTotal] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataAccsHistory
@Tbl CallCreditDataAccsHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataAccsHistory (
		[CallCreditDataAccsID],
		[M],
		[Bal],
		[CreditLimit],
		[Acc],
		[Pay],
		[StmtBal],
		[PayAmt],
		[CashAdvCount],
		[CashAdvTotal]
	) SELECT
		[CallCreditDataAccsID],
		[M],
		[Bal],
		[CreditLimit],
		[Acc],
		[Pay],
		[StmtBal],
		[PayAmt],
		[CashAdvCount],
		[CashAdvTotal]
	FROM @Tbl
END
GO


