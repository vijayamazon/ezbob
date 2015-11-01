IF OBJECT_ID('NL_LoanOptionsGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanOptionsGet AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[NL_LoanOptionsGet]
(@LoanID BIGINT)
AS
BEGIN
	SELECT
	   [LoanOptionsID]
      ,[AutoCharge]
      ,[StopAutoChargeDate]
      ,[AutoLateFees]
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
	FROM [dbo].[NL_LoanOptions]
	WHERE
		LoanID=@LoanID AND IsActive = 1
END
GO