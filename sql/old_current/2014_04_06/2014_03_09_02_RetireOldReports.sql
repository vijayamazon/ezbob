DELETE FROM ReportsUsersMap WHERE ReportID IN (SELECT Id FROM ReportScheduler WHERE Type IN ('RPT_MARKETING','RPT_NEW_CLIENT','RPT_IN_WIZARD','RPT_LINGER_CLIENT','RPT_MARKETPLACES_STATS','RPT_CALL_MANAGEMENT','RPT_OFFLINE_DAILY_REGISTRATIONS','RPT_OFFLINE_LEADS'))
GO
DELETE ReportArguments WHERE ReportId IN (SELECT Id FROM ReportScheduler WHERE Type IN ('RPT_MARKETING','RPT_NEW_CLIENT','RPT_IN_WIZARD','RPT_LINGER_CLIENT','RPT_MARKETPLACES_STATS','RPT_CALL_MANAGEMENT','RPT_OFFLINE_DAILY_REGISTRATIONS','RPT_OFFLINE_LEADS'))
GO
DELETE FROM ReportScheduler WHERE Type IN ('RPT_MARKETING','RPT_NEW_CLIENT','RPT_IN_WIZARD','RPT_LINGER_CLIENT','RPT_MARKETPLACES_STATS','RPT_CALL_MANAGEMENT','RPT_OFFLINE_DAILY_REGISTRATIONS','RPT_OFFLINE_LEADS')
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAdsReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAdsReport]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCustomerReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCustomerReport]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptMarketPlacesStats]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptMarketPlacesStats]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCallManagement]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCallManagement]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOfflineDailyRegistrations]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOfflineDailyRegistrations]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOfflineLeads]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOfflineLeads]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOnlineLeads]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOnlineLeads]
GO

UPDATE dbo.ReportScheduler
SET Type = 'RPT_LEADS'
	, Title = 'Online/Offline Leads'
	, StoredProcedure = 'RptLeads'
	, Header = 'Id,Reg Date,Credit Result,Email,Fullname,Wizard Step,Daytime Phone,Mobile Phone,Overall TurnOver,Segment'
	, Fields = '#Id,GreetingMailSentDate,CreditResult,Name,Fullname,WizardStepTypeDescription,DaytimePhone,MobilePhone,OverallTurnOver,Segment'
	, ToEmail = 'nimrodk@ezbob.com,ops@ezbob.com,rosb@ezbob.com,gilada@ezbob.com'
WHERE Type = 'RPT_ONLINE_LEADS'
GO
