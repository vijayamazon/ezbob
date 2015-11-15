IF OBJECT_ID('NL_UpdateLoanScheduleStatus') IS NOT NULL
	DROP PROCEDURE NL_UpdateLoanScheduleStatus
GO

CREATE PROCEDURE [dbo].[NL_UpdateLoanScheduleStatus]
	(@Id int,
	@Status varchar(50))
AS
BEGIN
	UPDATE [dbo].[NL_LoanSchedules]
   SET  [LoanScheduleStatusID] = (select LoanScheduleStatusID from NL_LoanScheduleStatuses where LoanScheduleStatus = @Status)
 WHERE LoanScheduleID = @Id

 SET NOCOUNT ON;
SELECT @@IDENTITY
END