IF OBJECT_ID('GetBrokerCommissionsForStatusUpdate') IS NULL
BEGIN
   EXECUTE('CREATE PROCEDURE GetBrokerCommissionsForStatusUpdate AS SELECT 1')
END 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE GetBrokerCommissionsForStatusUpdate
AS
BEGIN
	SELECT 
		lb.LoanBrokerCommissionID,
		lb.TrackingNumber, 
		lb.Description, 
		lb.BrokerID
	FROM 
		LoanBrokerCommission lb 
	WHERE 
		(lb.Status = 'InProgress' OR lb.Status IS NULL)
		AND lb.TrackingNumber IS NOT NULL
		
END
GO
