UPDATE dbo.ReportScheduler
SET Header = 'CustomerId,Fullname,Status,Principal,Balance,EU,Type Of Business,Residential Status,First Missed Repayment Date,Personal Address, Business Address'
	, Fields = '#CustomerId,Fullname,Status,Principal,Balance,EU,TypeOfBusiness,ResidentialStatus,FirstMissedRepaymentDate,PersonalAddress,CompanyAddress'
WHERE Type = 'RPT_CUSTOMER_STATUS'
GO