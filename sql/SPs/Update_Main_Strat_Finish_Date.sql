IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Update_Main_Strat_Finish_Date]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Update_Main_Strat_Finish_Date]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Update_Main_Strat_Finish_Date] 
	(@UserId int)
AS
BEGIN
	declare @MainStratFinishDate datetime  

set @MainStratFinishDate = GETUTCDATE()

UPDATE [dbo].[Customer]
   SET [LastStartedMainStrategyEndTime] = @MainStratFinishDate
 WHERE Id = @UserId

SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
