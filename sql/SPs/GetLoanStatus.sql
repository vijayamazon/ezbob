IF OBJECT_ID('GetLoanStatus') IS NULL
	EXECUTE('CREATE PROCEDURE GetLoanStatus AS SELECT 1')

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLoanStatus
	@LoanId INT
AS
BEGIN
	DECLARE @CustomerID INT = (SELECT 
								 CustomerId 
							   FROM 
							     Loan 
							   WHERE Id = @LoanId)
	
	DECLARE @WasLate BIT = (SELECT 
							  IsWasLate 
							FROM 
							  Customer 
							WHERE 
							  Id=@CustomerID)	   
							  					   
	DECLARE @BadStatuses INT = (SELECT count(*) 
								FROM 
									CustomerStatusHistory h 
								INNER JOIN 
									CustomerStatuses cs 
								ON
									h.PreviousStatus=cs.Id
								INNER JOIN 
									CustomerStatuses cs2 
								ON 
									h.NewStatus = cs.Id
								WHERE 
									CustomerId = @CustomerID 
									AND 
								 	(cs.IsWarning  = 1 
								 	 OR 
								 	 cs.IsDefault  = 1 
								 	 OR 
								 	 cs2.IsWarning = 1 
								 	 OR 
								 	 cs2.IsDefault = 1)
							   )

	SELECT 
		l.Status, l.Balance, l.LoanAmount, l.RefNum, CAST (CASE WHEN @BadStatuses>0 THEN 1 WHEN @WasLate = 1 THEN 1 ELSE 0 END AS BIT) WasLate
	FROM 
		loan l
	WHERE
		l.Id = @LoanId
END

GO