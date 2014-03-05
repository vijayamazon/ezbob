IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTasks_AddNew]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTasks_AddNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTasks_AddNew] 
	(@pAreaId int,
  @pName nvarchar(64),
  @pDescription nvarchar(1024))
AS
BEGIN
	DECLARE @id int;


  INSERT INTO StrategyTasks
  ([Name], [Description], [AreaId])
  VALUES
  (@pName, @pDescription, @pAreaId);

  set @id = @@IDENTITY;

  SELECT @id;
  RETURN @id
END
GO
