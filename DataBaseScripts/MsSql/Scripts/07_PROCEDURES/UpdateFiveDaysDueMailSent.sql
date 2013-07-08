IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFiveDaysDueMailSent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFiveDaysDueMailSent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateFiveDaysDueMailSent] 
(@Id int,
 @UpdateFiveDaysDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [FiveDaysDueMailSent] = @UpdateFiveDaysDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
