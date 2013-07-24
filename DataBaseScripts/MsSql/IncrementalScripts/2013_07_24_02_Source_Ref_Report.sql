UPDATE ReportScheduler SET
	Header='Type,Customer ID,Status,Loan Taken,Sign Up Date,Reference Source,Marketplaces,Wizard Step,Creation Date,Underwriter Decision,Manager Approved Sum,Loan Amount,Interest Rate,Loan Date,Full Name,Email',
	Fields='Type,!CustomerID,Status,LoanCount,GreetingMailSentDate,ReferenceSource,MarketPlaces,WizardStep,CreationDate,UnderwriterDecision,ManagerApprovedSum,LoanAmount,%InterestRate,LoanDate,FullName,Email'
WHERE
	Type='RPT_SOURCE_REF'
GO
