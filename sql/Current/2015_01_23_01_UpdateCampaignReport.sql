
UPDATE dbo.ReportScheduler
SET Header = 'Source,Medium,Term,Name,Registrations,Personal,Company,DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
	, Fields = 'Source,Medium,Term,Name,Registrations,Personal,Company,DataSources,Applications,NumOfApproved,NumOfRejected,RequestedAmount,NumOfLoans,LoanAmount'
	, ToEmail = 'stasd+report@ezbob.com,sivanc@ezbob.com,maayan@yellowhead.pro,eran@yellowhead.pro,gal@yellowheadinc.com,maya@yellowheadinc.com'
WHERE Type='RPT_CAMPAIGN_REPORT'
GO
