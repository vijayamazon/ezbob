DELETE FROM
	PacnetAgentConfigs
WHERE
	CfgKey LIKE 'PayPoint%'
GO

INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointMid', 'orange06')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointVpnPassword', 'ezbob2012')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointRemotePassword', 'ezbob2012')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointRetryCount', '5')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointSleepInterval', '30')
GO
