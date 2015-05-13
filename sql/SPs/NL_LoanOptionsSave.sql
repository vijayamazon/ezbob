SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanOptionsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanOptionsSave
GO

IF TYPE_ID('NL_LoanOptionsList') IS NOT NULL
	DROP TYPE NL_LoanOptionsList
GO

CREATE TYPE NL_LoanOptionsList AS TABLE (
	[LoanID] INT NOT NULL,
	[AutoPayment] BIT NOT NULL,
	[ReductionFee] BIT NOT NULL,
	[LatePaymentNotification] BIT NOT NULL,
	[CaisAccountStatus] NVARCHAR(50) NULL,
	[ManualCaisFlag] NVARCHAR(20) NULL,
	[EmailSendingAllowed] BIT NOT NULL,
	[SmsSendingAllowed] BIT NOT NULL,
	[MailSendingAllowed] BIT NOT NULL,
	[UserID] INT NULL,
	[InsertDate] DATETIME NOT NULL,
	[IsActive] BIT NOT NULL,
	[Notes] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE NL_LoanOptionsSave
@Tbl NL_LoanOptionsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanOptions (
		[LoanID],
		[AutoPayment],
		[ReductionFee],
		[LatePaymentNotification],
		[CaisAccountStatus],
		[ManualCaisFlag],
		[EmailSendingAllowed],
		[SmsSendingAllowed],
		[MailSendingAllowed],
		[UserID],
		[InsertDate],
		[IsActive],
		[Notes]
	) SELECT
		[LoanID],
		[AutoPayment],
		[ReductionFee],
		[LatePaymentNotification],
		[CaisAccountStatus],
		[ManualCaisFlag],
		[EmailSendingAllowed],
		[SmsSendingAllowed],
		[MailSendingAllowed],
		[UserID],
		[InsertDate],
		[IsActive],
		[Notes]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


