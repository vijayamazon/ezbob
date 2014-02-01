UPDATE dbo.ReportScheduler
SET Header = 'CashRequestId,CustomerId,System Decision,System Comment,Verification Decision,Verification Comment,System Calculated Sum,System Approved Sum,Verification Approved Sum,Css'
	, Fields = '!CashRequestId,#CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,SystemCalculatedSum,SystemApprovedSum,VerificationApprovedSum,{Css'
WHERE Type='RPT_AUTOMATION_VERIFICATION'
GO


ALTER PROCEDURE RptAutomationVerification
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT 1 AS CashRequestId, 1 AS CustomerId, 'test' AS SystemDecision, 'test' AS SystemComment,'test' AS VerificationDecision,'test' AS VerificationComment,0 AS SystemCalculatedSum,0 AS SystemApprovedSum,0 AS VerificationApprovedSum,'Failed unmatched' AS Css
END 

GO

