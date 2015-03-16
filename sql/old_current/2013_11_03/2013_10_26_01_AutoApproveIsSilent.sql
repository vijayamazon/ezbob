IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveIsSilent')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveIsSilent', 1, 'If 1 then auto approvals will be diverted to manual flow and a mail notification will be sent')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveSilentTemplateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveSilentTemplateName', 'AutoApproveSilentNotification', 'Template name in mandrill for the silent mail')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveSilentToAddress')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveSilentToAddress', 'yulys@ezbob.com', 'Address for silent mode of auto approval')
END

GO
