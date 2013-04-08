IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoansOverall]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoansOverall]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptLoansOverall
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
(
SELECT x.[Date] LoanDate, x.Name eMail, x.FirstName+' '+ x.Surname AS Name, sum(x.Repaid) Repaid, sum(x.NetoRepaid) AS PrincipalRepaid, sum(x.StillToPay) AS PrincipalOutstanding, sum(x.Given) AS OriginalLoanPrincipal, sum(x.Fees) Fees, sum(x.Interest) Interest, CASE WHEN sum(x.StillToPay)=0 THEN '0' ELSE '1' END AS LoanStatus 
FROM (

SELECT l.[Date], c.Name, c.FirstName, c.Surname, sum(lt.Amount) Repaid, (sum(lt.Amount)-(sum(lt.Fees)+sum(lt.Interest))) AS NetoRepaid, 0 StillToPay, 0 Given, sum(lt.Fees) Fees, sum(lt.Interest) Interest
FROM Loan l 
JOIN LoanTransaction lt ON lt.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE lt.Status='Done' AND 
lt.Type='PaypointTransaction' AND 
c.IsTest = 0
GROUP BY l.[Date], c.Name,c.FirstName, c.Surname

UNION

SELECT l.[Date], c.Name, c.FirstName, c.Surname, 0 Repaid, 0 NetoRepaid, 0 StillToPay, sum(lt.Amount)  Given, sum(lt.Fees) Fees, 0 Interest
FROM Loan l 
JOIN LoanTransaction lt ON lt.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE lt.Status='Done' AND 
lt.Type='PacnetTransaction' AND 
c.IsTest = 0
GROUP BY l.[Date], c.Name,c.FirstName, c.Surname 

UNION

SELECT l.[Date], c.Name, c.FirstName, c.Surname, 0 Repaid, 0 NetoRepaid, sum(ls.LoanRepayment) StillToPay, 0 Given, 0 Fees, 0 Interest
FROM Loan l 
JOIN LoanSchedule ls ON ls.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE ls.Status='StillToPay' OR ls.Status='Late' AND 
	  c.IsTest = 0
GROUP BY l.[Date], c.Name,c.FirstName, c.Surname 

) AS x
GROUP BY x.[Date], x.Name, x.FirstName, x.Surname 
)
UNION
(
SELECT NULL LoanDate, '' Name, '' Name, sum(x.Repaid) Repaid, sum(x.NetoRepaid) AS PrincipalRepaid, sum(x.StillToPay) AS PrincipalOutstanding, sum(x.Given) AS OriginalLoanPrincipal, sum(x.Fees) Fees, sum(x.Interest) Interest, '' AS LoanStatus 
FROM (

SELECT NULL LoanDate, '' Name, '' FirstName, '' Surname, sum(lt.Amount) Repaid, (sum(lt.Amount)-(sum(lt.Fees)+sum(lt.Interest))) AS NetoRepaid, 0 StillToPay, 0 Given, sum(lt.Fees) Fees, sum(lt.Interest) Interest
FROM Loan l 
JOIN LoanTransaction lt ON lt.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE lt.Status='Done' AND 
lt.Type='PaypointTransaction' AND 
c.IsTest = 0

UNION

SELECT NULL LoanDate, '' Name, '' FirstName, '' Surname, 0 Repaid, 0 NetoRepaid, 0 StillToPay, sum(lt.Amount)  Given, sum(lt.Fees) Fees, 0 Interest
FROM Loan l 
JOIN LoanTransaction lt ON lt.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE lt.Status='Done' AND 
lt.Type='PacnetTransaction' AND 
c.IsTest = 0

UNION

SELECT NULL LoanDate, '' Name, '' FirstName, '' Surname, 0 Repaid, 0 NetoRepaid, sum(ls.LoanRepayment) StillToPay, 0 Given, 0 Fees, 0 Interest
FROM Loan l 
JOIN LoanSchedule ls ON ls.LoanId = l.Id
JOIN Customer c ON l.CustomerId = c.Id 
WHERE ls.Status='StillToPay' OR ls.Status='Late' AND 
	  c.IsTest = 0

) AS x
)ORDER BY x.[Date]
END
GO
