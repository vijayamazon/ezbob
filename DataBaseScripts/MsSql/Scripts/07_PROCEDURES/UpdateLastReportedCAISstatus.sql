IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLastReportedCAISstatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLastReportedCAISstatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLastReportedCAISstatus] 
(@LoanId int
 ,@CAISStatus varchar(100)
 )


AS
BEGIN


		


UPDATE [dbo].[Loan]
   SET  LastReportedCAISStatus = @CAISStatus, 
		LastReportedCAISStatusDate	= GETUTCDATE() 
 WHERE Id = @LoanId



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
