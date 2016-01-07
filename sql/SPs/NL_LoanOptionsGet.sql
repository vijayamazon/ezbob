IF OBJECT_ID('NL_LoanOptionsGet') IS NOT NULL
	DROP PROCEDURE NL_LoanOptionsGet
GO

CREATE PROCEDURE [dbo].[NL_LoanOptionsGet]
(@LoanID BIGINT)
AS
BEGIN
	SELECT
		LoanID,
	   [LoanOptionsID]
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
	FROM [dbo].[NL_LoanOptions]
	WHERE
		LoanID=@LoanID AND IsActive = 1
END
GO
