UPDATE dbo.ReportScheduler
SET   Header = 'UserId,FirstName,Code,LotteryName,UserName,Prize,Status,CanWin,HasPlayed,LoanAmount,LoanDate,Css'
	, Fields = '#UserId,FirstName,Code,LotteryName,UserName,Prize,Status,CanWin,HasPlayed,LoanAmount,LoanDate,{Css'
WHERE Type='RPT_SCRATCH_CARD_LOTTERY'
GO
