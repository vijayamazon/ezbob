SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_PaymentsSave
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
	[Notes] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE NL_PaymentsSave
@Tbl NL_PaymentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Payments (
		[PaymentMethodID],		
		[PaymentTime],
		[Amount],
		[PaymentStatusID],
		[CreationTime],
		[CreatedByUserID],
		[DeletionTime],
		[DeletedByUserID],
		[Notes]
	) SELECT
		[PaymentMethodID],	
		[PaymentTime],
		[Amount],
		[PaymentStatusID],
		[CreationTime],
		[CreatedByUserID],
		[DeletionTime],
		[DeletedByUserID],
		[Notes]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


