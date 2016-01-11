SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoansSave') IS NOT NULL
	DROP PROCEDURE NL_LoansSave
GO

IF TYPE_ID('NL_LoansList') IS NOT NULL
	DROP TYPE NL_LoansList
GO

CREATE TYPE NL_LoansList AS TABLE (
	OfferID BIGINT NOT NULL,
	LoanTypeID INT NOT NULL,
	LoanStatusID INT NOT NULL,
	LoanFormulaID INT NOT NULL,		
	LoanSourceID INT NOT NULL,		
	EzbobBankAccountID INT NULL,
	CreationTime DATETIME NOT NULL,
	Refnum NVARCHAR(50) NOT NULL,			
	Position INT NOT NULL,
	DateClosed DATETIME NULL,	
	PrimaryLoanID BIGINT NULL,		
	OldLoanID INT NULL
)
GO
	
CREATE PROCEDURE NL_LoansSave
@Tbl NL_LoansList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Loans (
		OfferID ,
		LoanTypeID ,
		LoanStatusID ,
		LoanFormulaID ,		
		LoanSourceID ,		
		EzbobBankAccountID,
		CreationTime,
		Refnum ,			
		Position,
		DateClosed,	
		PrimaryLoanID,			
		OldLoanID
	) SELECT
		OfferID ,
		LoanTypeID ,
		LoanStatusID ,
		LoanFormulaID ,		
		LoanSourceID ,		
		EzbobBankAccountID,
		CreationTime,
		Refnum ,			
		Position,
		DateClosed,	
		PrimaryLoanID,		
		OldLoanID
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO
