SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[UIAT_C1359]
@brokerMail NVARCHAR(50)
AS
BEGIN
	UPDATE [ezbob].[dbo].[EzServiceCrontab] SET RepetitionTime=DATEADD(minute,1,GETDATE()) 
	WHERE JobID=(SELECT TOP 1 JobID FROM [ezbob].[dbo].[EzServiceCrontab] WHERE ActionNameID=221 ORDER BY JobID)

	WAITFOR DELAY '00:01:15'

	SELECT l.[LoanAmount]
		  ,lbc.[CardInfoID]
		  ,lbc.[CommissionAmount]
		  ,lbc.[PaidDate]
		  ,lbc.[TrackingNumber]
		  ,lbc.[Status]
		  ,lbc.[Description]
	FROM [ezbob].[dbo].[Broker] b,
		 [ezbob].[dbo].[LoanBrokerCommission] lbc,
		 [ezbob].[dbo].[Loan] l
	WHERE 
	l.Id=lbc.LoanID
	AND b.BrokerID=lbc.BrokerID
	AND b.contactEmail=@brokerMail
END