IF OBJECT_ID('UpdateBrokerCommissionTransferStatus') IS NULL
BEGIN
   EXECUTE('CREATE PROCEDURE UpdateBrokerCommissionTransferStatus AS SELECT 1')
END 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE UpdateBrokerCommissionTransferStatus
(
	@LoanBrokerCommissionID INT,
	@TrackingNumber nvarchar(100), 
	@TransactionStatus nvarchar(50),
	@Description nvarchar(100),
	@Now DATETIME = NULL
)
AS
BEGIN
	UPDATE 
		LoanBrokerCommission
    SET 
    	Status = @TransactionStatus,
    	Description = @Description,
    	TrackingNumber = @TrackingNumber
    WHERE
    	LoanBrokerCommissionID = @LoanBrokerCommissionID
    	
    IF @Now IS NOT NULL
    BEGIN
    	UPDATE 
    		LoanBrokerCommission
	    SET 
	    	PaidDate = @Now
	    WHERE
	    	LoanBrokerCommissionID = @LoanBrokerCommissionID
    END
END
GO