UPDATE dbo.ReportScheduler
SET Header = 'Broker Id,Name,Company,Mobile,Phone,Email,Signup Date,Num of clients added, Num of loans given, Value of loans given, Value of commission paid, Source Ref'
	, Fields = '^BrokerID,Name,Company,Mobile,Phone,Email,SignUpDate,NumOfClients,NumOfLoans,ValueOfLoans,ValueOfCommission,SourceRef'
WHERE Type='RPT_BROKER'
GO

UPDATE dbo.ReportScheduler
SET Header = 'Id,Status,Full name,Email,Daytime phone,Mobile phone,Registration date,Broker Id,Broker Name,Broker phone,Broker,Approved amount, Taken amount'
	, Fields = '#Id,Status,FullName,Name,DaytimePhone,MobilePhone,GreetingMailSentDate,^BrokerID,ContactName,ContactMobile,FirmName,ApprovedAmount, TakenAmount'
WHERE Type='RPT_BROKER_CLIENTS'
GO

UPDATE dbo.ReportScheduler
SET Header = 'CustomerId,Fullname,Status,Principal,EU'
	, Fields = '#CustomerId,Fullname,Status,Principal,EU'
WHERE Type='RPT_CUSTOMER_STATUS'
GO

UPDATE dbo.ReportScheduler
SET Header = 'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,EU,Setup Fee,Amount,Period,Planned Interest,Planned Repaid,Total Principal Repaid,Total Interest Repaid,Earned Interest,Expected Interest,Accrued Interest,Total Interest,Total Fees Repaid,Total Charges,Base Interest,Discount Plan,Customer Status,Level'
	, Fields = 'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,EU,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid,TotalPrincipalRepaid,TotalInterestRepaid,EarnedInterest,ExpectedInterest,AccruedInterest,TotalInterest,TotalFeesRepaid,TotalCharges,%BaseInterest,DiscountPlan,CustomerStatus,{RowLevel'
WHERE Type = 'RPT_LOANS_GIVEN'
GO
