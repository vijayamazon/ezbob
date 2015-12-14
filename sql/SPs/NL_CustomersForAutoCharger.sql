IF OBJECT_ID('NL_CustomersForAutoCharger') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_CustomersForAutoCharger AS SELECT 1')
GO


ALTER PROCEDURE NL_CustomersForAutoCharger 
@Now DATE
AS 
BEGIN

	IF OBJECT_ID('#nlCustomersForAutoCharger2') IS NOT NULL
	BEGIN
		truncate table #nlCustomersForAutoCharger2;
	END
	--select @Now;

	SELECT s.LoanScheduleID AS LoanScheduleId,
			l.LoanID,
			c.FirstName,
			c.Fullname,
			c.Id AS CustomerId,
			c.Name AS Email,
			c.TypeOfBusiness,
			s.PlannedDate AS DueDate,	
			l.RefNum,						
			lo.lastOptionID,
			h.LoanHistoryID,			
			l.OldLoanID			
	INTO #nlCustomersForAutoCharger2
	FROM Customer c
		JOIN vw_NL_LoansCustomer v ON v.CustomerID = c.Id
		JOIN NL_Loans l ON l.LoanID = v.LoanID
		JOIN CustomerStatuses cs ON (cs.Id = c.CollectionStatus AND cs.IsEnabled = 1 AND c.ExternalCollectionStatusID IS NULL)
		-- AutoCharge allowed for @Now
		JOIN (select LoanID, Max(LoanOptionsID) as lastOptionID from [dbo].[NL_LoanOptions] where [IsActive] = 1 AND (StopAutoChargeDate is null or StopAutoChargeDate > @Now) group by LoanID) lo ON lo.LoanID = l.LoanID
		JOIN [dbo].[NL_LoanHistory] h on h.LoanID = l.LoanID		
		JOIN NL_LoanSchedules s ON 
		(s.LoanHistoryID = h.LoanHistoryID AND s.PlannedDate >= (SELECT min(PlannedDate) FROM NL_LoanSchedules WHERE LoanHistoryID = s.LoanHistoryID AND DATEDIFF(DAY, s.PlannedDate, @Now) <= 30 ))
		JOIN NL_LoanScheduleStatuses ss on ss.LoanScheduleStatusID = s.LoanScheduleStatusID AND (ss.LoanScheduleStatus = 'StillToPay' OR ss.LoanScheduleStatus = 'Late') AND s.PlannedDate <= @Now
		LEFT JOIN [dbo].[NL_LoanRollovers] r on (r.LoanHistoryID = h.LoanHistoryID and r.[ExpirationTime] > @Now)
--	WHERE 	 
	--  s.PlannedDate = (SELECT min(PlannedDate) FROM NL_LoanSchedules WHERE LoanHistoryID = s.LoanHistoryID) AND  -- get the fisrt schedule from "still to pay" or "lates"		   
	  -- DATEDIFF(DAY, s.PlannedDate, @Now) <= 30 ---TODO CHECK WHAT SHOULD BE HERE	  
	--   s.PlannedDate >= (SELECT min(PlannedDate) FROM NL_LoanSchedules WHERE LoanHistoryID = s.LoanHistoryID AND DATEDIFF(DAY, s.PlannedDate, @Now) <= 30 );


	  SELECT LoanScheduleId,
			 LoanId,
			 FirstName,
			 Fullname,			 
			 CustomerId,
			 Email,
			 TypeOfBusiness,
			 DueDate,		
			(select ISNULL(PartialAutoCharging, 1) from [dbo].[NL_LoanOptions] where LoanOptionsID=t1.lastOptionID) AS ReductionFee,
			 RefNum,			
			 CAST(CASE WHEN (SELECT MAX(s1.PlannedDate) FROM NL_LoanSchedules s1 WHERE s1.LoanHistoryID = t1.LoanHistoryID) = t1.DueDate THEN 1 ELSE 0 END AS BIT) AS LastInstallment,
			OldLoanID				
	  FROM #nlCustomersForAutoCharger2 t1 
	 -- WHERE LoanScheduleId NOT IN
		--(SELECT LoanScheduleId
		-- FROM PaymentRollover
		-- WHERE ExpiryDate > @Now)
	ORDER BY t1.LoanScheduleId DESC ;
	


END