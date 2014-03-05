IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCollectionPayments]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCollectionPayments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCollectionPayments]
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT C.Id,C.Fullname,T.Amount,T.PostDate,L.LoanAmount,L.Balance
	FROM LoanTransaction T, Loan L, Customer C 
	WHERE C.IsTest = 0 
	AND C.Id = L.CustomerId 
	AND L.Id = T.LoanId 
	AND C.CollectionStatus IN (3,4,6,7) 
	AND T.PostDate >= @DateStart
	AND T.PostDate <= @DateEnd
	AND T.Status = 'Done' 
	AND T.Type = 'PaypointTransaction'
END
GO
