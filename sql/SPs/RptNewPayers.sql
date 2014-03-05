IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewPayers]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewPayers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewPayers]
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT C.Id,C.Fullname,L.LoanAmount,L.[Date]  
	FROM Customer C, Loan L 
	WHERE C.IsTest = 0 
	AND C.Id = L.CustomerId 
	AND L.[Date] >= dateadd(d,-35,@DateEnd)
	AND L.[Date] < dateadd(d,-30,@DateEnd) 
	AND C.Id IN (SELECT CustomerId
			     FROM Loan 
			     GROUP BY CustomerId 
			     HAVING count(1) = 1)
END
GO
