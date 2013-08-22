IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsSuccessfullyRegistered' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	DROP INDEX IX_Customer_Fill ON Customer
	DROP INDEX IX_Customer_IsRegistered ON Customer
	ALTER TABLE Customer DROP COLUMN IsSuccessfullyRegistered

	CREATE NONCLUSTERED INDEX [IX_Customer_Fill] ON [dbo].[Customer] 
	(
		[WizardStep] ASC,
		[IsTest] ASC
	)
	INCLUDE 
	( 
		[Id],
		[CreditResult],
		[FirstName],
		[MiddleInitial],
		[Surname],
		[DateOfBirth],
		[TimeAtAddress],
		[ResidentialStatus],
		[Gender],
		[MartialStatus],
		[TypeOfBusiness],
		[DaytimePhone],
		[MobilePhone],
		[Fullname],
		[OverallTurnOver],
		[WebSiteTurnOver]
	) 
	WITH 
	(
		PAD_INDEX = OFF, 
		STATISTICS_NORECOMPUTE = OFF, 
		SORT_IN_TEMPDB = OFF, 
		IGNORE_DUP_KEY = OFF, 
		DROP_EXISTING = OFF, 
		ONLINE = OFF, 
		ALLOW_ROW_LOCKS = ON, 
		ALLOW_PAGE_LOCKS = ON
	) 
	ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_Customer_IsRegistered] ON [dbo].[Customer] 
	(
		[WizardStep] ASC
	)
	INCLUDE ([CreditResult]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

END
GO
