IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_AddStrategyParam]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_AddStrategyParam]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_AddStrategyParam]
(
  @pTaskedStrategyId int,
  @pName nvarchar(256),
  @pDisplayName nvarchar(256),
  @pDescription nvarchar(1024),
  @pTypeName nvarchar(64),
  @pInitialValue nvarchar(256),
  @pConstraint nvarchar(1024)
)
AS
BEGIN

  DECLARE @id int;

  INSERT INTO TaskedStrategyParams
  ([TSId], [Name], [DisplayName], [Description], 
   [TypeName], [InitialValue], [ConstraintString])
  VALUES
  (@pTaskedStrategyId, @pName, @pDisplayName, @pDescription, 
   @pTypeName, @pInitialValue, @pConstraint);

  set @id = @@IDENTITY;

  SELECT @id;
  RETURN @id;

END
GO
