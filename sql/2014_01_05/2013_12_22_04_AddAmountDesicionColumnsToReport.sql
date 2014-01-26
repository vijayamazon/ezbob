UPDATE dbo.ReportScheduler
SET Header = 'CashRequestId,CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,SystemCalculatedSum,VerificationCalculatedSum,Css'
   ,Fields = '!CashRequestId,!CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,!SystemCalculatedSum,VerificationCalculatedSum,{Css'
WHERE Type = 'RPT_AUTOMATION_VERIFICATION' AND Header='CashRequestId,CustomerId,SystemDecision,SystemComment,VerificationDecision,VerificationComment,Css'
GO
