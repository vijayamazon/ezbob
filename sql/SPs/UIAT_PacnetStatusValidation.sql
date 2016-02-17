SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UIAT_PacnetStatusValidation') IS NULL
	EXECUTE('CREATE PROCEDURE UIAT_PacnetStatusValidation AS SELECT 1')
GO

ALTER PROCEDURE UIAT_PacnetStatusValidation
@brokerMail NVARCHAR(50)
AS
BEGIN
	SELECT l.LoanAmount
		  ,lbc.CardInfoID
		  ,lbc.CommissionAmount
		  ,lbc.PaidDate
		  ,lbc.TrackingNumber
		  ,lbc.Status
		  ,lbc.Description
	FROM Broker b,
		 LoanBrokerCommission lbc,
		 Loan l
	WHERE 
	l.Id=lbc.LoanID
	AND b.BrokerID=lbc.BrokerID
	AND b.contactEmail=@brokerMail
END
GO