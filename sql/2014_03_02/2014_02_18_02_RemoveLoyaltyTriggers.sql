IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_AccountCheckedLoyalty')
	DROP TRIGGER TR_AccountCheckedLoyalty
	
IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_CustomerLinkAccountLoyalty')
	DROP TRIGGER TR_CustomerLinkAccountLoyalty
	
IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_CustomerPersonalInfoLoyalty')
	DROP TRIGGER TR_CustomerPersonalInfoLoyalty
	
IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_CustomerSignupLoyalty')
	DROP TRIGGER TR_CustomerSignupLoyalty
	
IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_LoanTakenLoyalty')
	DROP TRIGGER TR_LoanTakenLoyalty
	
IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_RepaymentLoyalty')
	DROP TRIGGER TR_RepaymentLoyalty

GO

