IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoansToCollect]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLoansToCollect]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLoansToCollect] 
AS
BEGIN
	DECLARE @UtcNow DATE
	SELECT @UtcNow = CAST(GETUTCDATE() AS DATE)
	
	SELECT 
		ls.id, 
		ls.LoanId,
		CAST(
			CASE 
				WHEN 
					(SELECT MAX(ls1.Date) FROM LoanSchedule ls1 WHERE ls.LoanId = ls1.LoanId) = ls.date
				THEN 
					1 
				ELSE 
					0 
			END 
		AS BIT
		) AS LastInstallment,
		l.CustomerId, 
		ls.CustomInstallmentDate
	FROM 
		LoanSchedule ls
		LEFT JOIN Loan l ON 
			l.Id = ls.LoanId
	WHERE 
		ls.Status = 'StillToPay' AND 
		CAST(LS.Date AS DATE) <	@UtcNow
END
GO
