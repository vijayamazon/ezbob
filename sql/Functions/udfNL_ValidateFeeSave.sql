SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfNL_ValidateFeeSave') IS NOT NULL
	DROP FUNCTION dbo.udfNL_ValidateFeeSave
GO

CREATE FUNCTION  [dbo].[udfNL_ValidateFeeSave](
	@LoanID bigint,
	@AssignDate datetime
)
RETURNS NVARCHAR(64)
AS
BEGIN

	if @AssignDate < (select Min(h.EventTime) from NL_LoanHistory h where h.LoanID=@LoanID) 
		return 'Fee cannot be added before loan starts';

	if @AssignDate < (select MAX(s.PlannedDate) 
	from NL_LoanSchedules s join NL_LoanHistory h on h.LoanHistoryID=s.LoanHistoryID 
	where h.LoanID=@LoanID 
	and s.LoanScheduleStatusID = (select LoanScheduleStatusID from NL_LoanScheduleStatuses where LoanScheduleStatus='Paid'))
		return 'Fee cannot be added before paid installment';

	return NULL;

END
GO

