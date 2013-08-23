IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLoanScheduleCustomDate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLoanScheduleCustomDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanScheduleCustomDate] 
(@Id int)

AS
BEGIN

declare @CurDate date
set @CurDate = cast (GETUTCDATE() as DATE)

UPDATE [dbo].[LoanSchedule]
   SET  CustomInstallmentDate = @CurDate 
 WHERE Id = @Id


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
