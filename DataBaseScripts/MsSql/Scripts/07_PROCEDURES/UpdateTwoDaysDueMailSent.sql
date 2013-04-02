IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTwoDaysDueMailSent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTwoDaysDueMailSent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTwoDaysDueMailSent] 
(@Id int,
 @UpdateTwoDaysDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [TwoDaysDueMailSent] = @UpdateTwoDaysDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
