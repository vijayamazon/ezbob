IF EXISTS (SELECT * FROM PacnetAgentConfigs WHERE CfgKey='AutoRespondMandrillTemplate') 
	DELETE FROM PacnetAgentConfigs WHERE CfgKey='AutoRespondMandrillTemplate'
GO 	

IF NOT EXISTS (SELECT * FROM PacnetAgentConfigs WHERE CfgKey='AutoRespondMandrillWeekendTemplate') 	
	INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('AutoRespondMandrillWeekendTemplate', 'AutoresponderWeekend')
GO
	
IF NOT EXISTS (SELECT * FROM PacnetAgentConfigs WHERE CfgKey='AutoRespondMandrillNightTemplate') 	
	INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('AutoRespondMandrillNightTemplate', 'AutoresponderWeekend')
GO
