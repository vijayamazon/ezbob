IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllConfigurationVariables]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllConfigurationVariables]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllConfigurationVariables]
AS
BEGIN
	SELECT Name, Value FROM ConfigurationVariables
END
GO
