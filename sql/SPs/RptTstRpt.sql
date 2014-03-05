IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptTstRpt]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptTstRpt]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptTstRpt] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
AS
BEGIN
	SELECT 'Test report' AS SomeData, @DateStart AS FromTime, @DateEnd AS ToTime
END
GO
