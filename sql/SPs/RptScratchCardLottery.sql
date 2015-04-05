IF object_id('RptScratchCardLottery') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptScratchCardLottery AS SELECT 1')
END 
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptScratchCardLottery
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
SELECT  s.UserId,CASE WHEN c.FirstName IS NOT NULL THEN c.FirstName ELSE b.ContactName END AS FirstName, lc.Code, ll.LotteryName, s.UserName,
CASE 
	WHEN ll.LotteryID IN (3,4) AND p.Amount=1 THEN 'Barcelona' 
	WHEN ll.LotteryID IN (3,4) AND p.Amount=2 THEN 'Paris' 
	WHEN ll.LotteryID IN (3,4) AND p.Amount=3 THEN 'Rome' 
	ELSE CAST(p.Amount AS NVARCHAR(10)) END AS Prize, lps.Status, lps.CanWin, lps.HasPlayed,
 CASE WHEN sum(lo.LoanAmount) IS NULL THEN sum(lo2.LoanAmount) ELSE sum(lo.LoanAmount) END LoanAmount, 
 CASE WHEN max(lo.[Date]) IS NULL THEN max(lo2.Date) ELSE max(lo.Date) END LoanDate, 
 CASE WHEN lps.StatusID=4 AND p.Amount> 0 THEN 'success' ELSE '' END Css
FROM LotteryPlayers l 
INNER JOIN Security_User s ON s.UserId = l.UserID
LEFT JOIN LotteryPrizes p ON p.PrizeID = l.PrizeID
LEFT JOIN Lotteries ll ON ll.LotteryID=l.LotteryID
LEFT JOIN LotteryPlayerStatuses lps ON lps.StatusID=l.StatusID
LEFT JOIN LotteryCodes lc ON lc.CodeID = ll.CodeID
LEFT JOIN Loan lo ON s.UserId = lo.CustomerId
LEFT JOIN Customer cBro ON s.UserId=cBro.BrokerID
LEFT JOIN Loan lo2 ON cBro.Id = lo2.CustomerId
LEFT JOIN Broker b ON b.BrokerID = s.UserId
LEFT JOIN Customer c ON c.Id = s.UserId
WHERE s.UserName NOT LIKE '%ezbob%' AND lps.StatusID=4 AND p.Amount> 0 AND ll.IsActive=1
GROUP BY s.UserId, lc.Code,ll.LotteryID,ll.LotteryName, s.UserName, p.Amount, lps.Status, lps.CanWin, lps.HasPlayed, lps.StatusID, c.FirstName, b.ContactName
ORDER BY max(lo.[Date]),max(lo2.[Date])
END
GO
