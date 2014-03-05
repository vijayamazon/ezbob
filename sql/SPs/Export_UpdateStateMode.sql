IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_UpdateStateMode]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_UpdateStateMode]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_UpdateStateMode] 
	(@pApplicationId      INT,
    @pStatusMode         INT)
AS
BEGIN
	update export_results
		 set statusmode = @pStatusMode
	   where applicationid = @pApplicationId
END
GO
