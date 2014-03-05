IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyAreas_AddNew]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyAreas_AddNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyAreas_AddNew] 
	(@pName nvarchar(64),
  @pDescription nvarchar(1024))
AS
BEGIN
	DECLARE @id int;

  INSERT INTO StrategyAreas
  ([Name], [Description])
  VALUES
  (@pName, @pDescription);
  
  SET @id = @@IDENTITY;

  SELECT @id;
  RETURN @id
END
GO
