IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewLateClients]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewLateClients]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewLateClients] 
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT C.Id,C.Name,C.FirstName,C.Surname,AmountDue 
FROM LoanSchedule S,Customer C,Loan L 
WHERE 
	C.IsTest = 0 AND 
	C.Id = L.CustomerId AND 
	S.LoanId = L.Id AND 
	S.Date >= @DateStart AND 
	S.Date < @DateEnd AND 
	S.Status IN ('StillToPay','Late') AND 
	C.CollectionStatus in (0,6,10,11,12,13,14,15)
END

GO