IF OBJECT_ID('RptOnlineRegApp') IS NULL
	EXECUTE('CREATE PROCEDURE RptOnlineRegApp AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE RptOnlineRegApp
	(@DateStart DATETIME,
	 @DateEnd DATETIME)

AS
BEGIN

SET NOCOUNT ON

if OBJECT_ID('tempdb..#cashtemp1') is not NULL
BEGIN
	DROP TABLE #cashtemp1
END

if OBJECT_ID('tempdb..#cashtemp2') is not NULL
BEGIN
	DROP TABLE #cashtemp2
END


CREATE TABLE #out (
		Caption NVARCHAR(300)
		,RegDate SQL_VARIANT --datetime
		,Amount SQL_VARIANT --int
		,Css NVARCHAR(256) NULL
)

----------- NEW SU BY DATE (ONLINE) -------------
INSERT INTO #out(Caption, RegDate, Amount, Css)
		VALUES ('New Registrations By Date (Online)', 'RegDate', 'NumOfOnlineReg', 'total')

INSERT INTO #out (Caption, RegDate, Amount)
SELECT '' AS Caption, convert(DATE,GreetingMailSentDate) AS RegDate,COUNT(id) AS Amount
FROM  Customer
WHERE GreetingMailSentDate>=@DateStart AND GreetingMailSentDate<@DateEnd
				AND Id NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1)	
	  AND (IsOffline=0 OR IsOffline IS NULL)
GROUP BY convert(DATE,GreetingMailSentDate)

------------ NUMBER OF NEW APPLICATNTS (ONLINE) ---------------
INSERT INTO #out(Caption, RegDate, Amount, Css)
		VALUES ('Number of New Applications (Online)', 'ApplyDate', 'NumOfOnlineApp', 'total')

SELECT R.IdCustomer,DATEADD(day,DATEDIFF(day, 0,min(R.CreationDate)),0) AS FirstDateApply
INTO #cashtemp1
FROM CashRequests R
WHERE R.IdCustomer NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1)
GROUP BY R.IdCustomer

INSERT INTO #out (Caption, RegDate, Amount)
SELECT '' AS Caption, T.FirstDateApply AS RegDate,count(T.IdCustomer) AS Amount
FROM #cashtemp1 T
JOIN Customer C ON C.Id = T.IdCustomer
WHERE T.FirstDateApply BETWEEN @DateStart AND @DateEnd
	  AND (IsOffline=0 OR IsOffline IS NULL)
GROUP BY T.FirstDateApply
ORDER BY 1



----------- NEW SU BY DATE (OFFLINE) -----------
INSERT INTO #out(Caption, RegDate, Amount, Css)
		VALUES ('New Registrations By Date (Offline)', 'RegDate', 'NumOfOfflineReg', 'total')

INSERT INTO #out (Caption, RegDate, Amount)
SELECT '' AS Caption, convert(DATE,GreetingMailSentDate) AS RegDate,COUNT(id) AS Amount
FROM  Customer
WHERE GreetingMailSentDate>=@DateStart AND GreetingMailSentDate<@DateEnd
				AND Id NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1)	
	  AND IsOffline = 1
GROUP BY convert(DATE,GreetingMailSentDate)

------------ NUMBER OF NEW APPLICATNTS (OFFLINE) ---------------
INSERT INTO #out(Caption, RegDate, Amount, Css)
		VALUES ('Number of New Applications (Offline)', 'ApplyDate', 'NumOfOfflineApp', 'total')

SELECT R.IdCustomer,DATEADD(day,DATEDIFF(day, 0,min(R.CreationDate)),0) AS FirstDateApply
INTO #cashtemp2
FROM CashRequests R
WHERE R.IdCustomer NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1)
GROUP BY R.IdCustomer

INSERT INTO #out (Caption, RegDate, Amount)
SELECT '' AS Caption, T.FirstDateApply AS RegDate,count(T.IdCustomer) AS  Amount
FROM #cashtemp2 T
JOIN Customer C ON C.Id = T.IdCustomer
WHERE T.FirstDateApply BETWEEN @DateStart AND @DateEnd
	  AND C.IsOffline = 1
GROUP BY T.FirstDateApply
ORDER BY 1

-----------------------------------------------------

SELECT
		Caption,
		RegDate,
		Amount,
		Css
	FROM
		#out

-----------------------------------------------------

DROP TABLE #out

END
GO