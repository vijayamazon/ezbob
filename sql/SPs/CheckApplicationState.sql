IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckApplicationState]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CheckApplicationState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CheckApplicationState] 
	(@pApplicationId bigint)
AS
BEGIN
	DECLARE @lState bigint
  Select @lState = State from Application_Application where ApplicationId  = @pApplicationId;

 if @lState is NULL
	 RAISERROR ('ApplicationIdDoesNotExist', 11, 1);
	 
 if @lState = 0
     RAISERROR('ApplicationExecutingByEngine', 11, 1);
END
GO
