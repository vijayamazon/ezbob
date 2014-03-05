IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetExperianScore]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetExperianScore]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_GetExperianScore] 
	(@CustomerId INT)
AS
BEGIN
	SET NOCOUNT ON
	SELECT TOP 1 JsonPacket, ExperianScore 
	FROM MP_ExperianDataCache 
	WHERE CustomerId=@CustomerId AND ExperianScore IS NOT NULL AND DirectorId=0
	ORDER BY LastUpdateDate DESC
SET NOCOUNT OFF
END
GO
