IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ChannelGrabberServiceUrl')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ChannelGrabberServiceUrl', 'http://192.168.120.13', 'ChannelGrabber service url')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ChannelGrabberSleepTime')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ChannelGrabberSleepTime', '30', 'ChannelGrabber sleep time')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ChannelGrabberCycleCount')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ChannelGrabberCycleCount', '600', 'ChannelGrabber cycle count')
END
GO
