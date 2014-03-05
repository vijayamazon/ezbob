IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EzServiceGetDefaultInstance]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EzServiceGetDefaultInstance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EzServiceGetDefaultInstance] 
	(@Argument NVARCHAR(255))
AS
BEGIN
	SELECT
		i.InstanceID,
		i.InstanceName,
		i.SleepTimeout,
		i.AdminPort,
		i.ClientPort,
		i.HostName,
		i.ClientTimeoutSeconds
	FROM
		EzServiceInstance i
		INNER JOIN EzServiceDefaultInstance d ON i.InstanceID = d.InstanceID
	WHERE
		LOWER(d.Argument) = LOWER(@Argument)
END
GO
