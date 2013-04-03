IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReport_MarkAsRead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BehavioralReport_MarkAsRead]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BehavioralReport_MarkAsRead]
  @pReportId bigint
AS
BEGIN

	UPDATE BehavioralReports
	SET IsNotRead = 0
	WHERE Id = @pReportId;

END
GO
