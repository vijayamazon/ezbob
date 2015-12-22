IF OBJECT_ID('NL_GetCustomersXDaysDue') IS NOT NULL
	DROP PROCEDURE NL_GetCustomersXDaysDue
GO

CREATE PROCEDURE [dbo].[NL_GetCustomersXDaysDue]
	@Now DATE
AS
BEGIN
	DECLARE @PlusFive DATE;
	DECLARE @PlusTwo DATE;

	SET @PlusFive =  DATEADD(DAY, 5, @Now);
	SET @PlusTwo =  DATEADD(DAY, 2, @Now);
	
	
	SELECT 
		l.LoanID, 
		l.OldLoanID,
		c.FirstName, 
		s.PlannedDate, 
		p.CardNo CreditCardNo,
		c.Id AS CustomerId,		
		s.LoanScheduleID,
		s.TwoDaysDueMailSent,
		s.FiveDaysDueMailSent,
		Xdays = (
		CASE 
			WHEN s.PlannedDate = @PlusTwo  THEN 2 
			WHEN s.PlannedDate = @PlusFive THEN 5	
			ELSE 0
		END	)
	into #nlXdaysDue							
	FROM 			
		 NL_Loans l
		join vw_NL_LoansCustomer v on v.loanID = l.LoanID
		join Customer c	on v.CustomerID = c.Id
		join CustomerStatuses cs ON cs.Id = c.CollectionStatus AND cs.IsEnabled = 1
		Join NL_LoanHistory h on l.LoanID = h.LoanID 
		join NL_LoanSchedules s ON h.LoanHistoryID = s.LoanHistoryID
		Join NL_LoanScheduleStatuses ss
			on	ss.LoanScheduleStatus = 'StillToPay' AND (s.PlannedDate = @PlusTwo or s.PlannedDate = @PlusFive)				
		JOIN nl_LoanOptions lo ON lo.LoanId = l.LoanID and lo.StopAutoChargeDate IS NULL and lo.IsActive = 1
		LEFT JOIN PayPointCard p ON p.CustomerId=c.Id AND p.IsDefaultCard = 1	;		
	
	
	select * from #nlXdaysDue where Xdays=2 and TwoDaysDueMailSent=0 union select * from #nlXdaysDue where Xdays=5 and FiveDaysDueMailSent=0 ;
	
		
END