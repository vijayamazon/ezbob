IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetMails') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetMails
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetMails 
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EzbobMailTo') AS ToAddress,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EzbobMailCc') AS CcAddress
END
GO
