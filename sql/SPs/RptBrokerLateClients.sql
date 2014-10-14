IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptBrokerLateClients]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptBrokerLateClients]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptBrokerLateClients] 
	(@DateStart DATE,
	 @DateEnd DATE)
AS
BEGIN
	SELECT 
		C.Id,
		B.FirmName,
		C.Name,
		C.FirstName,
		C.Surname,
		SUM(LoanRepayment) AS PrincipalOwed
	FROM 
		LoanSchedule S,
		Customer C,
		Loan L,
		Broker B
	WHERE 
		C.IsTest = 0 AND 
		C.CreditResult = 'Late'	AND
		C.BrokerId IS NOT NULL AND
		C.BrokerId = B.BrokerID AND
		C.Id = L.CustomerId AND 
		S.LoanId = L.Id AND
		S.Status != 'Paid' AND
		S.Status != 'PaidEarly' AND
		S.Status != 'PaidOnTime'
	GROUP BY 
		C.Id,
		B.FirmName,
		C.Name,
		C.FirstName,
		C.Surname
END
GO
