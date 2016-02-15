SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanLegalsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanLegalsSave
GO

IF TYPE_ID('NL_LoanLegalsList') IS NOT NULL
	DROP TYPE NL_LoanLegalsList
GO

CREATE TYPE NL_LoanLegalsList AS TABLE (
	[OfferID] BIGINT NOT NULL,
	[SignatureTime] DATETIME NOT NULL,
	[RepaymentPeriod] INT NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[CreditActAgreementAgreed] BIT NULL,
	[PreContractAgreementAgreed] BIT NULL,
	[PrivateCompanyLoanAgreementAgreed] BIT NULL,
	[GuarantyAgreementAgreed] BIT NULL,
	[EUAgreementAgreed] BIT NULL,
	[COSMEAgreementAgreed] BIT NULL,
	[NotInBankruptcy] BIT NULL,
	[SignedName] NVARCHAR(128) NULL,
	[SignedLegalDocs] NVARCHAR(MAX)
)
GO

CREATE PROCEDURE NL_LoanLegalsSave
@Tbl NL_LoanLegalsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanLegals (
		[OfferID],
		[SignatureTime],
		[RepaymentPeriod],
		[Amount],
		[CreditActAgreementAgreed],
		[PreContractAgreementAgreed],
		[PrivateCompanyLoanAgreementAgreed],
		[GuarantyAgreementAgreed],
		[EUAgreementAgreed],
		[COSMEAgreementAgreed],
		[NotInBankruptcy],
		[SignedName],
		[SignedLegalDocs]
	) SELECT
		[OfferID],
		[SignatureTime],
		[RepaymentPeriod],
		[Amount],
		[CreditActAgreementAgreed],
		[PreContractAgreementAgreed],
		[PrivateCompanyLoanAgreementAgreed],
		[GuarantyAgreementAgreed],
		[EUAgreementAgreed],
		[COSMEAgreementAgreed],
		[NotInBankruptcy],
		[SignedName],
		[SignedLegalDocs]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


