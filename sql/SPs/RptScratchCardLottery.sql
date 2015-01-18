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
	SELECT  s.UserId, ll.LotteryName, s.UserName, p.Amount, lps.Status, lps.CanWin, lps.HasPlayed,
	 CASE WHEN sum(lo.LoanAmount) IS NULL THEN sum(lo2.LoanAmount) ELSE sum(lo.LoanAmount) END LoanAmount, 
	 CASE WHEN max(lo.[Date]) IS NULL THEN max(lo2.Date) ELSE max(lo.Date) END LoanDate, 
	 CASE WHEN lps.StatusID=4 AND p.Amount> 0 THEN 'success' ELSE '' END Css
	FROM LotteryPlayers l INNER JOIN Security_User s ON s.UserId = l.UserID
	LEFT JOIN LotteryPrizes p ON p.PrizeID = l.PrizeID
	LEFT JOIN Lotteries ll ON ll.LotteryID=l.LotteryID
	LEFT JOIN LotteryPlayerStatuses lps ON lps.StatusID=l.StatusID
	LEFT JOIN Loan lo ON s.UserId = lo.CustomerId
	LEFT JOIN Customer c ON s.UserId=c.BrokerID
	LEFT JOIN Loan lo2 ON c.Id = lo2.CustomerId
	WHERE s.UserName NOT LIKE '%ezbob%'
	GROUP BY s.UserId, ll.LotteryName, s.UserName, p.Amount, lps.Status, lps.CanWin, lps.HasPlayed, lps.StatusID
	ORDER BY max(lo.[Date]),max(lo2.[Date])
END 
GO
