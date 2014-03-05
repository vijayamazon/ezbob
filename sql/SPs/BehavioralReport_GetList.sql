IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReport_GetList]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BehavioralReport_GetList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BehavioralReport_GetList] 
	(@pStartDate datetime,
  @pEndDate datetime,
  @pTypeId int,
  @pStrategyName nvarchar(MAX),
  @pTestRun int)
AS
BEGIN
	DECLARE @l_startTypeID int,
          @l_endTypeID int,
          @l_startTestRun int,
          @l_endTestRun int;

  if @pTypeId = - 1 
    begin
      set @l_startTypeID = 0;
      set @l_endTypeID = 10000;
    end
  else
    begin
      set @l_startTypeID = @pTypeId;
      set @l_endTypeID = @pTypeId;
    end;

  if @pTestRun = - 1
    begin
      set @l_startTestRun = 0;
      set @l_endTestRun = 1;
    end
  else
    begin
      set @l_startTestRun = @pTestRun;
      set @l_endTestRun = @pTestRun;
    end;


  SELECT rep.Id, rep.StrategyId, rep.Name, s.DisplayName as StrategyName,
         rep.TypeId, rep.CreationDate, rep.TestRun, rep.IsNotRead
  FROM BehavioralReports rep, Strategy_Strategy s
  WHERE rep.StrategyId = s.StrategyId
	AND rep.TypeId >= @l_startTypeID AND rep.TypeId <= @l_endTypeID
	AND rep.CreationDate >= @pStartDate AND rep.CreationDate <= @pEndDate
	AND rep.TestRun >= @l_startTestRun AND rep.TestRun <= @l_endTestRun 
	AND s.IsDeleted = 0
	AND (s.DisplayName=@pStrategyName OR @pStrategyName='')
END
GO
