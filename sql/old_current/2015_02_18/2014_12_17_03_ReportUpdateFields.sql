UPDATE dbo.ReportScheduler
SET Header = 'CustomerId,EmailAddress,Fullname,SegmentType,CustomerStatus,NumOfLoansTook,PersonalScore,DatePaidLastLoan,DaytimePhone,MobilePhone,CRMNoteDate,CRMUsername,CRMComment,CRMStatus,CRMAction,BrokerOrNot'
    , Fields = '!CustomerId,EmailAddress,Fullname,SegmentType,CustomerStatus,NumOfLoansTook,PersonalScore,DatePaidLastLoan,DaytimePhone,MobilePhone,CRMNoteDate,CRMUsername,CRMComment,CRMStatus,CRMAction,BrokerOrNot'
WHERE Type = 'RPT_PAID_OFF_DIDNT_TAKE_NEW'
GO