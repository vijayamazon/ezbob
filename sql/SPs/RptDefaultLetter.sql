IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDefaultLetter]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDefaultLetter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptDefaultLetter
@DateStart DATE,
@DateEnd DATE
AS
SELECT C.Id,C.Name,C.FirstName,C.Surname,AmountDue 
FROM LoanSchedule S, Customer C, Loan L, CustomerStatuses CS
WHERE 
	C.IsTest = 0 AND 
	C.Id = L.CustomerId AND 
	S.LoanId = L.Id AND 
	dateadd(d,14,S.Date) >= @DateStart AND 
	dateadd(d,14,S.Date) < @DateEnd AND 
	S.Status IN ('Late') AND 
	C.CollectionStatus = CS.Id AND 
	CS.Name != 'Disabled' AND
	CS.IsDefault = 0

GO
