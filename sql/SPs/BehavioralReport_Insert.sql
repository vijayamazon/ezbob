IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReport_Insert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BehavioralReport_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BehavioralReport_Insert] 
	(@pStrategyId int,
@pReportName nvarchar(1024),
@pReportTypeId int,
@pPath nvarchar(2048),
@pIsTestRun int,
@pReportId bigint output)
AS
BEGIN
	DECLARE @l_ReportId int;

insert into [BehavioralReports]
(StrategyId, Name, TypeId, Path, CreationDate, TestRun, IsNotRead)
values
(@pStrategyID, @pReportName, @pReportTypeId, @pPath, GETDATE(), @pIsTestRun, 1)

set @pReportId = @@IDENTITY
END
GO
