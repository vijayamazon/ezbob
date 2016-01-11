IF OBJECT_ID('NL_PaymentCancel') IS NOT NULL
	EXECUTE('DROP PROCEDURE NL_PaymentCancel')
GO

IF OBJECT_ID('NL_PaymentsSave') IS NOT NULL
	EXECUTE('DROP PROCEDURE NL_PaymentsSave')
GO

IF TYPE_ID('NL_PaymentsList') IS NOT NULL
	DROP TYPE NL_PaymentsList
GO

CREATE TYPE NL_PaymentsList AS TABLE (
	[PaymentMethodID] INT NOT NULL,	
	[PaymentTime] DATETIME NOT NULL,
	[Amount] DECIMAl(18,6) NOT NULL,
	[PaymentStatusID] INT NOT NULL,
	[CreationTime] DATETIME NOT NULL,
	[CreatedByUserID] INT NULL,
	[DeletionTime] DATETIME NULL,
	[DeletedByUserID] INT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[LoanID] INT NOT NULL
)
GO
