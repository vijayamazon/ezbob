IF OBJECT_ID('NL_LateLoansGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_LateLoansGet AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_LateLoansGet]
@Now DATE
AS
BEGIN
	SELECT 
		s.LoanScheduleID, 
		l.LoanID ,
		l.[OldLoanID],
		v.CustomerID,  
		lst.LoanStatus LoanStatus,
		ss.LoanScheduleStatus ScheduleStatus, 
		s.PlannedDate	
	FROM 
		NL_LoanSchedules s INNER JOIN NL_LoanHistory h ON h.LoanHistoryID =s.LoanHistoryID
		INNER JOIN NL_Loans l on h.LoanID = l.LoanID
		INNER JOIN vw_NL_LoansCustomer v on v.LoanID = l.LoanID
		INNER JOIN NL_LoanScheduleStatuses ss on ss.LoanScheduleStatusID = s.LoanScheduleStatusID
		INNER JOIN NL_LoanStatuses lst on lst.LoanStatusID = l.LoanStatusID
	WHERE 
		(ss.LoanScheduleStatus = 'StillToPay' OR ss.LoanScheduleStatus = 'Late') AND CONVERT(DATE, s.PlannedDate) < @Now;

END