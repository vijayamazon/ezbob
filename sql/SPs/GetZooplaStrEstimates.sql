IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetZooplaStrEstimates]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetZooplaStrEstimates]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetZooplaStrEstimates]
AS
BEGIN
	SELECT Id, ZooplaEstimate FROM Zoopla
END
GO
