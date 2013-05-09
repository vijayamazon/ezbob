IF OBJECT_ID ('dbo.PacnetAgentConfigs') IS NOT NULL
BEGIN
	PRINT 'PacnetAgentConfigs exists'	
END
ELSE
BEGIN
CREATE TABLE dbo.PacnetAgentConfigs
(
	CfgKey VARCHAR(100) NOT NULL,
	CfgValue VARCHAR(100),
	CONSTRAINT PK_PacnetAgentConfigs PRIMARY KEY (CfgKey)
)
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

INSERT INTO PacnetAgentConfigs VALUES ('MailboxReconnectionIntervalSeconds', '10')
INSERT INTO PacnetAgentConfigs VALUES ('LoginAddress', 'pacnetreport@ezbob.com')
INSERT INTO PacnetAgentConfigs VALUES ('LoginPassword', 'ezbob2012$')
INSERT INTO PacnetAgentConfigs VALUES ('Port', '993')
INSERT INTO PacnetAgentConfigs VALUES ('Server', 'imap.gmail.com')
INSERT INTO PacnetAgentConfigs VALUES ('MailBeeLicenseKey', 'MN700-E9212517212D217021D6F428312E-52A1')

GO


