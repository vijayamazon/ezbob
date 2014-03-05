IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAutomationVerification]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAutomationVerification]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAutomationVerification] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
AS
BEGIN
	SELECT 1 AS CashRequestId, 1 AS CustomerId, 'test' AS SystemDecision, 'test' AS SystemComment,'test' AS VerificationDecision,'test' AS VerificationComment,0 AS SystemCalculatedSum,0 AS SystemApprovedSum,0 AS VerificationApprovedSum,'Failed unmatched' AS Css
END
GO
