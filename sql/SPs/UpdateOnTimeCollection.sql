IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateOnTimeCollection]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateOnTimeCollection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateOnTimeCollection] 
	(@LoanId int,
 @OnTime Numeric(18),
 @OnTimeNum int)
AS
BEGIN
	UPDATE [dbo].[Loan]
   SET  OnTime = @OnTime,
	    OnTimeNum = @OnTime

   
 WHERE Id = @LoanId



 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
