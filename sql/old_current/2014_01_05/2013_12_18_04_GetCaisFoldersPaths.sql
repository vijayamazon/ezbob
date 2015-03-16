IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCaisFoldersPaths]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCaisFoldersPaths]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCaisFoldersPaths]
AS
BEGIN
	SELECT 
		(SELECT value FROM ConfigurationVariables WHERE Name = 'CAISPath') AS CaisPath,
		(SELECT value FROM ConfigurationVariables WHERE Name = 'CAISPath2') AS CaisPath2
END
GO
