SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanOptionsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanOptionsSave
GO

IF TYPE_ID('NL_LoanOptionsList') IS NOT NULL
	DROP TYPE NL_LoanOptionsList
GO

CREATE TYPE NL_LoanOptionsList AS TABLE (
	[LoanID] BIGINT NOT NULL,
	[StopAutoChargeDate] DATETIME NULL,
	[PartialAutoCharging] BIT NOT NULL,
	[LatePaymentNotification] BIT NOT NULL,
	[CaisAccountStatus] NVARCHAR(50) NULL,
	[ManualCaisFlag] NVARCHAR(20) NULL,
	[EmailSendingAllowed] BIT NOT NULL,
	[MailSendingAllowed] BIT NOT NULL,
	[SmsSendingAllowed] BIT NOT NULL,
	[UserID] INT NULL,
	[InsertDate] DATETIME NOT NULL,
	[IsActive] BIT NOT NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[StopLateFeeFromDate] DATETIME NULL,
	[StopLateFeeToDate] DATETIME NULL
)
GO

CREATE PROCEDURE NL_LoanOptionsSave
@Tbl NL_LoanOptionsList READONLY,
@LoanID INT
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE NL_LoanOptions SET IsActive=0 WHERE LoanID=@LoanID

	INSERT INTO NL_LoanOptions (
       [LoanID]
      ,[StopAutoChargeDate]
      ,[PartialAutoCharging]
      ,[LatePaymentNotification]
      ,[CaisAccountStatus]
      ,[ManualCaisFlag]
      ,[EmailSendingAllowed]
      ,[MailSendingAllowed]
      ,[SmsSendingAllowed]
      ,[UserID]
      ,[InsertDate]
      ,[IsActive]
      ,[Notes]
	  ,[StopLateFeeFromDate]
	  ,[StopLateFeeToDate]
	) SELECT
       [LoanID]
      ,[StopAutoChargeDate]
      ,[PartialAutoCharging]
      ,[LatePaymentNotification]
      ,[CaisAccountStatus]
      ,[ManualCaisFlag]
      ,[EmailSendingAllowed]
      ,[MailSendingAllowed]
      ,[SmsSendingAllowed]
      ,[UserID]
      ,[InsertDate]
      ,[IsActive]
      ,[Notes]
	  ,[StopLateFeeFromDate]
	  ,[StopLateFeeToDate]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO