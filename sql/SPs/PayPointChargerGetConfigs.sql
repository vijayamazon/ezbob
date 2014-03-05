IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayPointChargerGetConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[PayPointChargerGetConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PayPointChargerGetConfigs]
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'AmountToChargeFrom') AS AmountToChargeFrom
END
GO
