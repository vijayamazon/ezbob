IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDailyStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDailyStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*-------------------------------*/
Create PROCEDURE RptDailyStats
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
if OBJECT_ID('tempdb..#TodayClients') is not NULL 
BEGIN
	DROP TABLE #TodayClients
END
if OBJECT_ID('tempdb..#PastClients') is not NULL 
BEGIN
	DROP TABLE #PastClients
END
if OBJECT_ID('tempdb..#NewClients') is not NULL 
BEGIN
	DROP TABLE #NewClients
END
if OBJECT_ID('tempdb..#NewOffers') is not NULL 
BEGIN
	DROP TABLE #NewOffers
END
if OBJECT_ID('tempdb..#ReportPart1') is not NULL 
BEGIN
	DROP TABLE #ReportPart1
END
if OBJECT_ID('tempdb..#ReportPart2') is not NULL 
BEGIN
	DROP TABLE #ReportPart2
END

	SELECT DISTINCT IdCustomer INTO #TodayClients FROM CashRequests WHERE CreationDate >= @DateStart AND CreationDate < @DateEnd
	SELECT DISTINCT C.IdCustomer INTO #PastClients FROM CashRequests C,#TodayClients T WHERE CreationDate < @DateStart AND C.IdCustomer = T.IdCustomer
	SELECT IdCustomer INTO #NewClients FROM #TodayClients WHERE IdCustomer NOT IN (SELECT IdCustomer FROM #PastClients)
	SELECT C.IdCustomer,min(Id) OfferId INTO #NewOffers FROM CashRequests C,#NewClients N WHERE C.IdCustomer = N.IdCustomer GROUP BY c.IdCustomer
	
	SELECT 'Applications' Line, 'Total' Type, count(1) Counter,UnderwriterDecision,sum(ManagerApprovedSum) Value  INTO #ReportPart1 FROM CashRequests  WHERE CreationDate >= @DateStart AND CreationDate < @DateEnd GROUP BY UnderwriterDecision
	INSERT INTO #ReportPart1 SELECT 'Applications' Line,'New' Type,count(1) Counter,UnderwriterDecision,sum(ManagerApprovedSum) Value FROM CashRequests  WHERE CreationDate >= @DateStart AND CreationDate < @DateEnd AND Id IN (SELECT OfferId FROM #NewOffers) GROUP BY UnderwriterDecision
	INSERT INTO #ReportPart1 SELECT 'Applications' Line,'Old' Type,count(1) Counter,UnderwriterDecision,sum(ManagerApprovedSum) Value FROM CashRequests  WHERE CreationDate >= @DateStart AND CreationDate < @DateEnd AND Id NOT IN (SELECT OfferId FROM #NewOffers) GROUP BY UnderwriterDecision

	SELECT 'Loans' Line,'Total' Type,count(1) Counter,'' AS UnderwriterDescision, sum(LoanAmount) Value INTO #ReportPart2 FROM Loan WHERE DATE >= @DateStart AND DATE < @DateEnd
	INSERT INTO #ReportPart2 SELECT 'Loans' Line,'Old' Type,count(1) Counter,'' AS UnderwriterDescision, sum(LoanAmount) Value FROM Loan WHERE DATE >= @DateStart AND DATE < @DateEnd AND CustomerId IN (SELECT CustomerId FROM Loan WHERE DATE < @DateStart)
	INSERT INTO #ReportPart2 SELECT 'Loans' Line,'New' Type,count(1) Counter,'' AS UnderwriterDescision, sum(LoanAmount) Value FROM Loan WHERE DATE >= @DateStart AND DATE < @DateEnd AND CustomerId NOT IN (SELECT CustomerId FROM Loan WHERE DATE < @DateStart)


	SELECT * FROM #ReportPart1
	UNION	
	SELECT * FROM #ReportPart2
	UNION
	SELECT 'Payments' Line,'' Type,count(1) Counter,'' AS UnderwriterDescision,sum(Amount) Value FROM LoanTransaction WHERE PostDate >= @DateStart AND PostDate < @DateEnd AND Type = 'PaypointTransaction' AND Description = 'payment from customer'
	UNION
   	SELECT 'Registers' Line,'' Type,count(1) Counter,'' AS UnderwriterDescision, 0 Value FROM Customer WHERE GreetingMailSentDate >= @DateStart AND GreetingMailSentDate < @DateEnd 
	
END
GO
