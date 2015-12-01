SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UIAT_C1359') IS NULL
	EXECUTE('CREATE PROCEDURE UIAT_C1359 AS SELECT 1')
GO

ALTER PROCEDURE UIAT_C1359
@brokerMail NVARCHAR(50)
AS
BEGIN
	SELECT l.[LoanAmount]
		  ,lbc.[CardInfoID]
		  ,lbc.[CommissionAmount]
		  ,lbc.[PaidDate]
		  ,lbc.[TrackingNumber]
	FROM [ezbob].[dbo].[Broker] b,
		 [ezbob].[dbo].[LoanBrokerCommission] lbc,
		 [ezbob].[dbo].[Loan] l
	WHERE 
	l.Id=lbc.LoanID
	AND b.BrokerID=lbc.BrokerID
	AND b.contactEmail=@brokerMail
END