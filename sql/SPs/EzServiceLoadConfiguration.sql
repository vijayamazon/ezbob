IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EzServiceLoadConfiguration]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EzServiceLoadConfiguration]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EzServiceLoadConfiguration] 
	(@InstanceName NVARCHAR(32))
AS
BEGIN
	SELECT
		InstanceID,
		SleepTimeout,
		AdminPort,
		ClientPort,
		HostName,
		ClientTimeoutSeconds
	FROM
		EzServiceInstance
	WHERE
		InstanceName = @InstanceName
END
GO
