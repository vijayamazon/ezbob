IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_AUTOMATION_VERIFICATION')
BEGIN 
INSERT INTO dbo.ReportScheduler
	(
	Type
	, Title
	, StoredProcedure
	, IsDaily
	, IsWeekly
	, IsMonthly
	, Header
	, Fields
	, ToEmail
	, IsMonthToDate
	)
VALUES
	(
	'RPT_AUTOMATION_VERIFICATION'
	, 'Automation Verification'
	, 'RptAutomationVerification'
	, 0
	, 0
	, 0
	, 'CashRequestId,CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,Css'
	, '!CashRequestId,!CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,{Css'
	, 'stasd@ezbob.com,nimrodk@ezbob.com,eilaya@ezbob.com'
	, 0
	)
END 
GO

IF NOT EXISTS (SELECT * FROM ReportArguments ra WHERE ra.ReportArgumentNameId=1 AND ra.ReportId = (SELECT Id FROM ReportScheduler WHERE Type='RPT_AUTOMATION_VERIFICATION'))
BEGIN 
	INSERT INTO ReportArguments(ReportArgumentNameId,ReportId) VALUES (1, (SELECT Id FROM ReportScheduler WHERE Type='RPT_AUTOMATION_VERIFICATION'))
END 	
GO 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAutomationVerification]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RptAutomationVerification]
GO

CREATE PROCEDURE RptAutomationVerification
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT 1 AS CashRequestId, 1 AS CustomerId, 'test' AS SystemDecision, 'test' AS SystemComment,'test' AS VerificationDecision,'test' AS VerificationComment,'Failed unmatched' AS Css
END 
GO 
