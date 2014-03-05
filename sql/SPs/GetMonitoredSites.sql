IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMonitoredSites]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMonitoredSites]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMonitoredSites]
AS
BEGIN
	SELECT 
		Site 
	FROM 
		MonitoredSites
END
GO
