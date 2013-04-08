IF OBJECT_ID ('dbo.MonitoredSites') IS NOT NULL
BEGIN
	PRINT 'MonitoredSites exists'
	DROP TABLE MonitoredSites
END

CREATE TABLE dbo.MonitoredSites
(
	Site VARCHAR(300) NOT NULL,
	CONSTRAINT PK_MonitoredSites PRIMARY KEY (Site)
)

INSERT INTO MonitoredSites VALUES ('http://www.ezbob.com')
--INSERT INTO MonitoredSites VALUES ('http://App.ezbob.com')

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
