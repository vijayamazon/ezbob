IF OBJECT_ID ('dbo.SupportAgentConfigs') IS NOT NULL
BEGIN
    RETURN
END
ELSE
BEGIN
	CREATE TABLE dbo.SupportAgentConfigs
	(
		CfgKey VARCHAR(100) NOT NULL,
		CfgValue VARCHAR(100),
		CONSTRAINT PK_SupportAgentConfigs PRIMARY KEY (CfgKey)
	)

	INSERT INTO SupportAgentConfigs VALUES ('NoMailThresholdInMinutes', '90')
	INSERT INTO SupportAgentConfigs VALUES ('NoMailThresholdInMinutesNight', '360')
	INSERT INTO SupportAgentConfigs VALUES ('NoMailThresholdInMinutesWeekend', '600')
	INSERT INTO SupportAgentConfigs VALUES ('FromAddress', 'monitor@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('FromPassword', 'ezbob2013')
	INSERT INTO SupportAgentConfigs VALUES ('MailBeeLicenseKey', 'MN700-E9212517212D217021D6F428312E-52A1')
	INSERT INTO SupportAgentConfigs VALUES ('LoginAddress', 'monitor@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('LoginPassword', 'ezbob2013')
	INSERT INTO SupportAgentConfigs VALUES ('Server', 'imap.gmail.com')
	INSERT INTO SupportAgentConfigs VALUES ('Port', '993')
	INSERT INTO SupportAgentConfigs VALUES ('MinimumIntervalInMinutes', '50')
	INSERT INTO SupportAgentConfigs VALUES ('AdditionalRandomMinutes', '20')
	INSERT INTO SupportAgentConfigs VALUES ('MainLoopIntervalSeconds', '240')
	INSERT INTO SupportAgentConfigs VALUES ('HourToSendIAmAlive', '6')
	INSERT INTO SupportAgentConfigs VALUES ('HourToSendSummary', '20')
	INSERT INTO SupportAgentConfigs VALUES ('StartOfDayHour', '8')
	INSERT INTO SupportAgentConfigs VALUES ('MailboxReconnectionIntervalSeconds', '10')
	INSERT INTO SupportAgentConfigs VALUES ('EndOfDayHour', '20')

	INSERT INTO SupportAgentConfigs VALUES ('ToAddressSummary', 'tech@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('ToAddressSiteMonitor', 'yulys@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('ToAddressIsLive', 'yulys@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('ToAddressMailError', 'yulys@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('ToAddressNoMailError', 'yulys@ezbob.com')

	INSERT INTO SupportAgentConfigs VALUES ('NewCustomerThresholdInMinutesWeekend', '1440')
	INSERT INTO SupportAgentConfigs VALUES ('NewCustomerThresholdInMinutesNight', '720')
	INSERT INTO SupportAgentConfigs VALUES ('NewCustomerThresholdInMinutes', '480')
	INSERT INTO SupportAgentConfigs VALUES ('ToAddressNoNewCustomers', 'yulys@ezbob.com')
	INSERT INTO SupportAgentConfigs VALUES ('MaxCustomerId', '0')

	SET ANSI_NULLS ON
	SET QUOTED_IDENTIFIER ON
END
GO
