IF OBJECT_ID('NL_GetCustomersXDaysDue') IS NOT NULL
	DROP PROCEDURE NL_GetCustomersXDaysDue
GO

CREATE PROCEDURE NL_GetCustomersXDaysDue
	@Now DATETIME	
AS
BEGIN
	DECLARE @TodayPlusFive DATE
	DECLARE @TodayPlusTwo DATE

	SET @TodayPlusFive = DateAdd(dd,2 ,GETUTCDATE())
	SET @TodayPlusTwo = DateAdd(dd,5 ,GETUTCDATE())

	SELECT 
		l.LoanID, 
		c.FirstName, 
		ls.PlannedDate, 
		p.CardNo CreditCardNo,
		c.Id AS CustomerId,
		Xdays = (
			CASE WHEN
			 		ls.PlannedDate <= @TodayPlusTwo AND ls.TwoDaysDueMailSent = 0  
					THEN 2 
				WHEN 
					ls.PlannedDate <= @TodayPlusFive AND ls.FiveDaysDueMailSent = 0  
					THEN 5	
			ELSE 0
			END
			),
			ls.LoanScheduleID									
	FROM 			
		 NL_Loans l
		join vw_NL_LoansCustomer v on v.loanID = l.LoanID
		join Customer c	on v.CustomerID = c.Id
		join CustomerStatuses cs ON cs.Id = c.CollectionStatus AND cs.IsEnabled = 1
		Join NL_LoanHistory lh	on l.LoanID = lh.LoanID 
		join NL_LoanSchedules ls ON ls.LoanHistoryID = ls.LoanHistoryID
		Join NL_LoanScheduleStatuses lss
			on	(lss.LoanScheduleStatus = 'StillToPay') AND					
				ls.PlannedDate  >= @Now AND
				(
					(			
						ls.PlannedDate  <= @TodayPlusTwo AND ls.TwoDaysDueMailSent = 0  
					)				
					OR
					(				
						ls.PlannedDate  <= @TodayPlusFive AND ls.FiveDaysDueMailSent = 0
					)				
				)		
		JOIN nl_LoanOptions lo ON lo.LoanId = l.LoanID and lo.StopAutoChargeDate IS NULL and lo.IsActive = 1
		LEFT JOIN PayPointCard p ON p.CustomerId=c.Id AND p.IsDefaultCard = 1		
END