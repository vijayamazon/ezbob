IF OBJECT_ID('NL_LoanSchedulesUpdate') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulesUpdate
GO

CREATE PROCEDURE NL_LoanSchedulesUpdate
	@LoanScheduleID BIGINT,	
	@LoanScheduleStatusID INT = NULL,	
	@PlannedDate DATETIME = '1900-01-01 00:00:00',
	@ClosedTime DATETIME = '1900-01-01 00:00:00',
	@TwoDaysDueMailSent BIT = NULL,
	@FiveDaysDueMailSent BIT = NULL
AS
BEGIN
    UPDATE NL_LoanSchedules
    SET 
	LoanScheduleStatusID=ISNULL(@LoanScheduleStatusID,LoanScheduleStatusID), 
	PlannedDate = CASE WHEN (@PlannedDate <> '1900-01-01 00:00:00') THEN @PlannedDate ELSE PlannedDate END,			  
	ClosedTime = CASE WHEN (@ClosedTime <> '1900-01-01 00:00:00') THEN @ClosedTime ELSE ClosedTime END,
	TwoDaysDueMailSent=ISNULL(@TwoDaysDueMailSent,TwoDaysDueMailSent), 
	FiveDaysDueMailSent=ISNULL(@FiveDaysDueMailSent,FiveDaysDueMailSent)
    WHERE LoanScheduleID = @LoanScheduleID
END