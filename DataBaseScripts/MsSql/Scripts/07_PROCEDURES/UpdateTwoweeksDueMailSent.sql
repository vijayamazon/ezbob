IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTwoWeeksDueMailSent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTwoWeeksDueMailSent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTwoWeeksDueMailSent] 
(@Id int,
 @UpdateTwoWeeksDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [TwoWeeksDueMailSent] = @UpdateTwoWeeksDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
