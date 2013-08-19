IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLoanScheduleStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLoanScheduleStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanScheduleStatus] 
(@Id int,
 @Status varchar(50))

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [Status] = @Status
 WHERE Id = @Id


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
