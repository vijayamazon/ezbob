SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanInterestFreezeDeactivate') IS NOT NULL
	DROP PROCEDURE NL_LoanInterestFreezeDeactivate
GO

CREATE PROCEDURE NL_LoanInterestFreezeDeactivate
@UserId INT,
@LoanInterestFreezeID BIGINT,
@DeactivationDate DATETIME
AS
BEGIN
	UPDATE NL_LoanInterestFreeze 
	set DeactivationDate = @DeactivationDate,
	DeletedByUserID =@UserId	
	where LoanInterestFreezeID = @LoanInterestFreezeID 
END
GO



