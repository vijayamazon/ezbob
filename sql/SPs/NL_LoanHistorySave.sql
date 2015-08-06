SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanHistorySave') IS NOT NULL
	DROP PROCEDURE NL_LoanHistorySave
GO

IF TYPE_ID('NL_LoanHistoryList') IS NOT NULL
	DROP TYPE NL_LoanHistoryList
GO

CREATE TYPE NL_LoanHistoryList AS TABLE (	
	[LoanID]  [BIGINT] NOT NULL,
	[UserID] [int] NULL,
	[LoanLegalID] [BIGINT] NULL,
	[Amount] [decimal](18, 6) NOT NULL,
	[RepaymentCount] [int] NOT NULL,
	[InterestRate] [decimal](18, 6) NULL,	
	[EventTime] [datetime] NOT NULL,		
	[Description] NVARCHAR(MAX) NULL,
	[AgreementModel] NVARCHAR(MAX) NULL
);
GO

CREATE PROCEDURE NL_LoanHistorySave
@Tbl NL_LoanHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanHistory (
	[LoanID] ,
	[UserID]  ,
	[LoanLegalID] ,
	[Amount] ,
	[RepaymentCount],
	[InterestRate],	
	[EventTime] ,
	[Description]   ,	
	[AgreementModel] 	
	) SELECT
		[LoanID] ,
	[UserID]  ,
	[LoanLegalID] ,
	[Amount] ,
	[RepaymentCount],
	[InterestRate],	
	[EventTime] ,
	[Description]   ,	
	[AgreementModel] 
	 
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO