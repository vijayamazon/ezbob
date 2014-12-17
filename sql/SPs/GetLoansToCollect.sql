IF OBJECT_ID('GetLoansToCollect') IS NULL
	EXECUTE('CREATE PROCEDURE GetLoansToCollect AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetLoansToCollect]
@Now DATETIME
AS
BEGIN
	DECLARE @UtcNow DATE
	SELECT @UtcNow = CAST(@Now AS DATE)
	
	SELECT 
		ls.Id, 
		ls.LoanId,
		CAST(
			CASE 
				WHEN 
					(SELECT MAX(ls1.Date) FROM LoanSchedule ls1 WHERE ls.LoanId = ls1.LoanId) = ls.[Date]
				THEN 
					1 
				ELSE 
					0 
			END 
		AS BIT
		) AS LastInstallment,
		l.CustomerId, 
		l.Status LoanStatus,
		ls.Status ScheduleStatus, 
		ls.[Date] ScheduleDate,
		ls.Interest
	FROM 
		LoanSchedule ls
		INNER JOIN Loan l ON 
			l.Id = ls.LoanId
	WHERE 
		(ls.Status = 'StillToPay' OR ls.Status = 'Late')
		AND
		CAST(LS.Date AS DATE) <	@UtcNow
END

GO
