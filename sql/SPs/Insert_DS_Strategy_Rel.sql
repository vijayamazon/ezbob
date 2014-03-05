IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Insert_DS_Strategy_Rel]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Insert_DS_Strategy_Rel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Insert_DS_Strategy_Rel] 
	(@pDataSourceName nvarchar(255),
    @pStrategyId int)
AS
BEGIN
	DECLARE @newId int;
  set @newId = null

  SELECT @newId = Id 
  FROM DataSource_StrategyRel 
    WHERE DataSourceName = @pDataSourceName 
	  AND StrategyId = @pStrategyId;

  if @newId is null
  begin
  	insert into DATASOURCE_STRATEGYREL
	(DataSourceName, StrategyId)
  	values
  	(@pDataSourceName, @pStrategyId)
  END
END
GO
