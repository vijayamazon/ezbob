----------------------------------------------------------Permissions------------------------------------------------------------
--update description of existing permission
UPDATE Security_Permission SET [Description]='editEmail button' WHERE Name='EmailConfirmationButton'
UPDATE Security_Permission SET [Description]='change customer status / external customer status' WHERE Name='CustomerStatus'
UPDATE Security_Permission SET [Description]='change customer IsTest' WHERE Name='TestUser'
UPDATE Security_Permission SET [Description]='Add crm notes (develop)' WHERE Name='CRM'
UPDATE Security_Permission SET [Description]='new credit line button' WHERE Name='NewCreditLineButton'
UPDATE Security_Permission SET [Description]='edit offer button' WHERE Name='CreditLineFields'
UPDATE Security_Permission SET [Description]='click approve reject buttons' WHERE Name='ApproveReject'
UPDATE Security_Permission SET [Description]='click escalate button' WHERE Name='Escalate'
UPDATE Security_Permission SET [Description]='recheck mps, renew token, recheck askville' WHERE Name='RecheckMarketplaces'
UPDATE Security_Permission SET [Description]='check bank account' WHERE Name='CheckBankAccount'
UPDATE Security_Permission SET [Description]='run experian consumer,company,aml checks' WHERE Name='RunCreditBureauChecks'
UPDATE Security_Permission SET [Description]='aml bwa info buttons in messaging tab' WHERE Name='SendingMessagesToClients'
UPDATE Security_Permission SET [Description]='edit loan' WHERE Name='EditLoanDetails'
UPDATE Security_Permission SET [Description]='open bug' WHERE Name='OpenBug'
UPDATE Security_Permission SET [Description]='add bank' WHERE Name='AddBankAccount'
UPDATE Security_Permission SET [Description]='change default bank' WHERE Name='ChangeBankAccount'
UPDATE Security_Permission SET [Description]='add paypoint' WHERE Name='AddDebitCard'
UPDATE Security_Permission SET [Description]='add rollover' WHERE Name='Rollover'
UPDATE Security_Permission SET [Description]='add manual payment' WHERE Name='MakePayment'
UPDATE Security_Permission SET [Description]='change loan options' WHERE Name='LoanOptions'
UPDATE Security_Permission SET [Description]='skip auto decision' WHERE Name='AvoidAutomaticDecision'
UPDATE Security_Permission SET [Description]='click suspend button' WHERE Name='SuspendBtn'
UPDATE Security_Permission SET [Description]='change discount plan (need to implement)' WHERE Name='DiscountPlan'
UPDATE Security_Permission SET [Description]='click disable shop button' WHERE Name='DisableShop'
UPDATE Security_Permission SET [Description]='hides broker client`s details like phone number' WHERE Name='HideBrokerClientDetails'
UPDATE Security_Permission SET [Description]='click return from pending status button' WHERE Name='ReturnBtn'
UPDATE Security_Permission SET [Description]='click change button' WHERE Name='ChangePhone'
UPDATE Security_Permission SET [Description]='Rescheduling "Outside Loan Payment Arrangement"' WHERE Name='RescheduleOutOfLoanButton'

--removing RecheckPayPal,RerunningMarketplaces,RerunningCreditCheck,PacnetManualButton permission
IF EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RecheckPayPal')
BEGIN
	DECLARE @PermissionId INT = (SELECT Id FROM Security_Permission WHERE Name = 'RecheckPayPal')
	DELETE FROM Security_RolePermissionRel WHERE PermissionId = @PermissionId
	DELETE FROM Security_Permission WHERE Id = @PermissionId
END
GO

IF EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RerunningMarketplaces')
BEGIN
	DECLARE @PermissionId INT = (SELECT Id FROM Security_Permission WHERE Name = 'RerunningMarketplaces')
	DELETE FROM Security_RolePermissionRel WHERE PermissionId = @PermissionId
	DELETE FROM Security_Permission WHERE Id = @PermissionId
END
GO

IF EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RerunningCreditCheck')
BEGIN
	DECLARE @PermissionId INT = (SELECT Id FROM Security_Permission WHERE Name = 'RerunningCreditCheck')
	DELETE FROM Security_RolePermissionRel WHERE PermissionId = @PermissionId
	DELETE FROM Security_Permission WHERE Id = @PermissionId
END
GO

IF EXISTS (SELECT * FROM Security_Permission WHERE Name = 'PacnetManualButton')
BEGIN
	DECLARE @PermissionId INT = (SELECT Id FROM Security_Permission WHERE Name = 'PacnetManualButton')
	DELETE FROM Security_RolePermissionRel WHERE PermissionId = @PermissionId
	DELETE FROM Security_Permission WHERE Id = @PermissionId
END
GO

/*adding permissions: 
			existing: EmailConfirmationButton,CustomerStatus,TestUser,CRM,NewCreditLineButton,CreditLineFields,ApproveReject,Escalate,RecheckMarketplaces,CheckBankAccount,
					  RunCreditBureauChecks,SendingMessagesToClients,EditLoanDetails,OpenBug,AddBankAccount,ChangeBankAccount,AddDebitCard,Rollover,MakePayment,LoanOptions,
					  AvoidAutomaticDecision,SuspendBtn,DiscountPlan,DisableShop,HideBrokerClientDetails,ReturnBtn,ChangePhone,RescheduleOutOfLoanButton,
				new:  OldEditLoanDetails,DownloadOffer,RecalculateMedal,BlockTakingLoan,CCIMark,FraudStatus,TrustPilot,ParseBankTransactions,EnterHMRC,
					  YodleeSearchWords,YodleeRules,DebitCardCustomerSelection,ChangeAddress,ChangeCompany,SendDocuments,AddEditDirector,UploadFile,DeleteFile,
					  SendSMS,RecheckFraud,AddProperty,LandRegistry,ZooplaRecheck,AddFraudUser,GenerateCAIS,EditCAISFile,AutomationAndSettings,AddFunds,PacnetRequests,
					  ResetPassword,ResetBrokerPassword,FinishWizard,CreateLoan,ChangeBrokerEmail,ChangeBroker,BrokerWhiteLabel,CreateInvestor,ManageInvestor,
					  InvestorAccounting,InvestorConfig,FindInvestor,ForceInvestor,AddLogbookEntry,LegalAddEditReview,LegalConfirm,EditPermissions,UnderwriterDashboard,CloseEditBug
*/


--------------------------------------------existing permissions-----------------------------------

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'EmailConfirmationButton')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'EmailConfirmationButton','editEmail button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CustomerStatus')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CustomerStatus','change customer status / external customer status')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'TestUser')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'TestUser','change customer IsTest')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CRM')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CRM','Add crm notes (develop)')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'NewCreditLineButton')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'NewCreditLineButton','new credit line button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CreditLineFields')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CreditLineFields','edit offer button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ApproveReject')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ApproveReject','click approve reject buttons')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'Escalate')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'Escalate','click escalate button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RecheckMarketplaces')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'RecheckMarketplaces','recheck mps, renew token, recheck askville')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CheckBankAccount')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CheckBankAccount','check bank account')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RunCreditBureauChecks')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'RunCreditBureauChecks','run experian consumer,company,aml checks')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'SendingMessagesToClients')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'SendingMessagesToClients','aml bwa info buttons in messaging tab')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'EditLoanDetails')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'EditLoanDetails','edit loan')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'OpenBug')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'OpenBug','open bug')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddBankAccount')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddBankAccount','add bank')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangeBankAccount')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangeBankAccount','change default bank')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddDebitCard')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddDebitCard','add paypoint')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'Rollover')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'Rollover','add rollover')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'MakePayment')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'MakePayment','add manual payment')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'LoanOptions')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'LoanOptions','change loan options')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AvoidAutomaticDecision')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AvoidAutomaticDecision','skip auto decision')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'SuspendBtn')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'SuspendBtn','click suspend button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DiscountPlan')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'DiscountPlan','change discount plan (need to implement)')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DisableShop')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'DisableShop','click disable shop button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'HideBrokerClientDetails')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'HideBrokerClientDetails','hides broker client`s details like phone number')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ReturnBtn')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ReturnBtn','click return from pending status button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangePhone')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangePhone','click change button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RescheduleOutOfLoanButton')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'RescheduleOutOfLoanButton','Rescheduling "Outside Loan Payment Arrangement"')
END    
GO


--------------------------------------------new permissions-----------------------------------

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'OldEditLoanDetails')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'OldEditLoanDetails','edit loan free style')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DownloadOffer')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'DownloadOffer','click download offer button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RecalculateMedal')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'RecalculateMedal','click recalculate medal button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'BlockTakingLoan')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'BlockTakingLoan','change block taking loan button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CCIMark')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CCIMark','change CCI mark')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'FraudStatus')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'FraudStatus','change fraud status')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'TrustPilot')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'TrustPilot','change trust pilot')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ParseBankTransactions')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ParseBankTransactions','parse bank transactions button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'EnterHMRC')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'EnterHMRC','enter manually HMRC')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'YodleeSearchWords')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'YodleeSearchWords','yodlee add search words')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'YodleeRules')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'YodleeRules','yodlee change rules')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DebitCardCustomerSelection')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'DebitCardCustomerSelection','customer can select default debit card')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangeAddress')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangeAddress','cross check change current address')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangeCompany')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangeCompany','change company')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'SendDocuments')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'SendDocuments','send documents to sign BR and PG')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddEditDirector')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddEditDirector','Add / Edit directors')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'UploadFile')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'UploadFile','upload file')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DeleteFile')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'DeleteFile','delete file')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'SendSMS')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'SendSMS','send SMS')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'RecheckFraud')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'RecheckFraud','recheck fraud button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddProperty')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddProperty','add property button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'LandRegistry')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'LandRegistry','landregistry button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ZooplaRecheck')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ZooplaRecheck','zoopla recheck button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddFraudUser')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddFraudUser','add fraud user')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'GenerateCAIS')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'GenerateCAIS','generate cais button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'EditCAISFile')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'EditCAISFile','edit cais file')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AutomationAndSettings')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AutomationAndSettings','all changes of automation and settings')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddFunds')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddFunds','Add funds button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'PacnetRequests')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'PacnetRequests','pacnet topup and confirmation buttons')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ResetPassword')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ResetPassword','reset customer password')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ResetBrokerPassword')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ResetBrokerPassword','reset broker password')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'FinishWizard')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'FinishWizard','finish wizard button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CreateLoan')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CreateLoan','create loan hidden button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangeBrokerEmail')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangeBrokerEmail','change broker email')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangeBroker')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ChangeBroker','change assigned to customer broker')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'BrokerWhiteLabel')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'BrokerWhiteLabel','add / change broker white label')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CreateInvestor')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CreateInvestor','create investor')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ManageInvestor')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ManageInvestor','manage invertor')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'InvestorAccounting')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'InvestorAccounting','manage investor accounting ')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'InvestorConfig')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'InvestorConfig','')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'FindInvestor')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'FindInvestor','find investor button')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ForceInvestor')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'ForceInvestor','force assigning loan to investor')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AddLogbookEntry')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'AddLogbookEntry','add logbook entry')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'LegalAddEditReview')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'LegalAddEditReview','legal docs review add and edit')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'LegalConfirm')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'LegalConfirm','legal docs confirm')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'EditPermissions')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'EditPermissions','super user edit permissions tool (to develop)')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'UnderwriterDashboard')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'UnderwriterDashboard','access to UW dashboard')
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'CloseEditBug')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission (Id, Name, Description) VALUES (@id,'CloseEditBug','close or edit bug')
END    
GO

------------------------------------------------------Roles------------------------------------------------------------

--removing crm,CollectorRO role
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'crm')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'crm')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'CollectorRO')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'CollectorRO')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO
 
--adding existing Admin,SuperUser,Web,Underwriter,manager,Collector,JuniorUnderwriter,Sales,BrokerSales,CollectorSenior,CollectorManager,InvestorWeb role
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Admin')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('Admin','Administrator - Manage users')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'SuperUser')
BEGIN 
	INSERT INTO Security_Role(Name, [Description]) VALUES('SuperUser', 'SuperUser - Have rights to all applications')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Web')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('Web','Web')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Underwriter')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('Underwriter','Underwriter')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'manager')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('manager','Manager')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Collector')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('Collector','Collector - Allow change only Loans')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'JuniorUnderwriter')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('JuniorUnderwriter','Junior Underwriter')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Sales')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('Sales','Sales person')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'BrokerSales')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('BrokerSales','Broker Sales person')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'CollectorSenior')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('CollectorSenior','')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'CollectorManager')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('CollectorManager','')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'InvestorWeb')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('InvestorWeb','Investor web contact')
END 
GO

--adding  new SalesForceSales, CollectionIFrame role
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'SalesForceSales')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('SalesForceSales','Just for the sf iframe ')
END 
GO

IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'CollectionIFrame')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('CollectionIFrame','Just for collection iframe (to develop)')
END 
GO

--adding  new ReadOnly role with user relations
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'ReadOnly')
BEGIN 
	INSERT INTO Security_Role (Name,[Description]) VALUES ('ReadOnly','User with access to UW without any functionality')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_UserRoleRelation(UserId, RoleId) 
	SELECT u.UserId, @roleId FROM Security_User u
	WHERE u.UserName IN ('gadif','hagayj','inas','lauren','marius','masha','paulj','sharonep','shirik','sivanc','tomerg','amiyc','darrenh','vitasd')
	
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('UnderwriterDashboard')
END 
GO

--adding new Accounting role with permissions relations
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Accounting')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, [Description]) VALUES('Accounting', 'For Bat-el')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('CheckBankAccount','OpenBug','AddBankAccount','ChangeBankAccount','AddDebitCard','MakePayment',
	'UploadFile','CRM','ManageInvestor','InvestorAccounting','AddFunds','PacnetRequests','UnderwriterDashboard')
END 
GO

--adding CustomerCare role with permissions relations
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'CustomerCare')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, [Description]) VALUES('CustomerCare', 'customer care')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('EmailConfirmationButton','CustomerStatus','CRM','RecheckMarketplaces','OpenBug','AddBankAccount','ChangeBankAccount','AddDebitCard',
	'DisableShop','ChangePhone','DownloadOffer','TrustPilot','ChangeAddress','SendDocuments','AddEditDirector','UploadFile','SendSMS','ResetPassword','UnderwriterDashboard')
END 
GO

--adding Legal role with permissions relations
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Legal')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, [Description]) VALUES('Legal', 'Jason for legal agreements editing')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('LegalAddEditReview','OpenBug')
END 
GO

--roles   
DECLARE @Accounting INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Accounting')
DECLARE @BrokerSales INT = (SELECT RoleId FROM Security_Role WHERE Name = 'BrokerSales')
DECLARE @Collector INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Collector')
DECLARE @CollectorManager INT = (SELECT RoleId FROM Security_Role WHERE Name = 'CollectorManager')
DECLARE @CollectorSenior INT = (SELECT RoleId FROM Security_Role WHERE Name = 'CollectorSenior')
DECLARE @CustomerCare INT = (SELECT RoleId FROM Security_Role WHERE Name = 'CustomerCare')
DECLARE @JuniorUnderwriter INT = (SELECT RoleId FROM Security_Role WHERE Name = 'JuniorUnderwriter')
DECLARE @manager INT = (SELECT RoleId FROM Security_Role WHERE Name = 'manager')
DECLARE @Sales INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Sales')
DECLARE @SuperUser INT = (SELECT RoleId FROM Security_Role WHERE Name = 'SuperUser')
DECLARE @Underwriter INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Underwriter')
DECLARE @Legal INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Legal')
/* adding BrokerSales role relations
						existing:	with ChangePhone,EmailConfirmationButton,CRM,CheckBankAccount,OpenBug,SuspendBtn,ReturnBtn permissions
							 new:	with ChangeBrokerEmail,ChangeBroker,BrokerWhiteLabel,DownloadOffer,TrustPilot,ChangeAddress,SendDocuments,
									AddEditDirector,UploadFile,SendSMS,ResetBrokerPassword,ChangeBrokerEmail,ChangeBroker permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @BrokerSales, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('ChangePhone','EmailConfirmationButton','CRM','CheckBankAccount','OpenBug','SuspendBtn','ReturnBtn',
'ChangeBrokerEmail','ChangeBroker','BrokerWhiteLabel','DownloadOffer','TrustPilot','ChangeAddress','SendDocuments',
'AddEditDirector','UploadFile','SendSMS','ResetBrokerPassword','ChangeBrokerEmail','ChangeBroker','UnderwriterDashboard')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@BrokerSales)

/* adding Collector role relations
						existing:	with ChangePhone,RescheduleOutOfLoanButton,Rollover,MakePayment,LoanOptions,CRM,CheckBankAccount,
									OpenBug,AddBankAccount,ChangeBankAccount,AddDebitCard,CustomerStatus,EditLoanDetails permissions
							new:	with LoanOptions,DownloadOffer,CCIMark,DebitCardCustomerSelection,UploadFile,SendSMS permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @Collector, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('ChangePhone','RescheduleOutOfLoanButton','Rollover','MakePayment','LoanOptions','CRM','CheckBankAccount','OpenBug','AddBankAccount','ChangeBankAccount','AddDebitCard','CustomerStatus','EditLoanDetails',
'LoanOptions','DownloadOffer','CCIMark','DebitCardCustomerSelection','UploadFile','SendSMS','UnderwriterDashboard')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@Collector)

/* adding manager role relations 
						existing:	with EmailConfirmationButton,CustomerStatus,TestUser,CRM,NewCreditLineButton,CreditLineFields,ApproveReject,RecheckMarketplaces,CheckBankAccount,
									RunCreditBureauChecks,SendingMessagesToClients,OpenBug,AddBankAccount,ChangeBankAccount,AddDebitCard,RerunningMarketplaces,EditLoanDetails,
									ChangePhone,Rollover,MakePayment,LoanOptions,AvoidAutomaticDecision,SuspendBtn,ReturnBtn,DiscountPlan,DisableShop permissions
							new:	with RescheduleOutOfLoanButton,DownloadOffer,RecalculateMedal,BlockTakingLoan,CCIMark,FraudStatus,TrustPilot,
									ParseBankTransactions,EnterHMRC,YodleeSearchWords,YodleeRules,DebitCardCustomerSelection,ChangeAddress,ChangeCompany,SendDocuments,AddEditDirector,
									UploadFile,DeleteFile,SendSMS,RecheckFraud,AddProperty,LandRegistry,ZooplaRecheck,AddFraudUser,GenerateCAIS,EditCAISFile,AutomationAndSettings,AddFunds,
									PacnetRequests,ResetPassword,ResetBrokerPassword,FinishWizard,ChangeBrokerEmail,ChangeBroker,BrokerWhiteLabel,CreateInvestor,ManageInvestor,
									InvestorAccounting,InvestorConfig,FindInvestor,ForceInvestor,AddLogbookEntry,LegalAddEditReview,LegalConfirm permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @manager, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('EmailConfirmationButton','CustomerStatus','TestUser','CRM','NewCreditLineButton','CreditLineFields','ApproveReject','RecheckMarketplaces',
'CheckBankAccount','RunCreditBureauChecks','SendingMessagesToClients','OpenBug','AddBankAccount','ChangeBankAccount','AddDebitCard','RerunningMarketplaces',
'EditLoanDetails','ChangePhone','Rollover','MakePayment','LoanOptions','AvoidAutomaticDecision','SuspendBtn','ReturnBtn','DiscountPlan','DisableShop',
'RescheduleOutOfLoanButton','DownloadOffer','RecalculateMedal','BlockTakingLoan','CCIMark','FraudStatus','TrustPilot',
'ParseBankTransactions','EnterHMRC','YodleeSearchWords','YodleeRules','DebitCardCustomerSelection','ChangeAddress',
'ChangeCompany','SendDocuments','AddEditDirector','UploadFile','DeleteFile','SendSMS','RecheckFraud','AddProperty',
'LandRegistry','ZooplaRecheck','AddFraudUser','GenerateCAIS','EditCAISFile','AutomationAndSettings','AddFunds','PacnetRequests',
'ResetPassword','ResetBrokerPassword','FinishWizard','ChangeBrokerEmail','ChangeBroker','BrokerWhiteLabel','CreateInvestor',
'ManageInvestor','InvestorAccounting','InvestorConfig','FindInvestor','ForceInvestor','AddLogbookEntry','LegalAddEditReview','LegalConfirm','UnderwriterDashboard','CloseEditBug')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@manager)

/* adding CollectorManager role existing relation with RescheduleOutOfLoanButton permission*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @CollectorManager, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('RescheduleOutOfLoanButton')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@CollectorManager)

/* adding CollectorSenior role existing relation with RescheduleOutOfLoanButton permission*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @CollectorSenior, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('RescheduleOutOfLoanButton')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@CollectorSenior)

/* adding JuniorUnderwriter role relations
								existing:	with RunCreditBureauChecks,CRM,NewCreditLineButton,Escalate,RecheckMarketplaces,OpenBug,SuspendBtn,
											ReturnBtn,CreditLineFields,ApproveReject,DiscountPlan,DisableShop permissions
									new:	with ChangePhone,DownloadOffer,RecalculateMedal,ParseBankTransactions,EnterHMRC,YodleeSearchWords,ChangeAddress,
											ChangeCompany,SendDocuments,AddEditDirector,UploadFile,SendSMS,RecheckFraud,AddProperty,
											LandRegistry,ZooplaRecheck,AddFunds,PacnetRequests permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @JuniorUnderwriter, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('RunCreditBureauChecks','CRM','NewCreditLineButton','Escalate','RecheckMarketplaces','OpenBug','SuspendBtn','ReturnBtn','CreditLineFields','ApproveReject',
'DiscountPlan','DisableShop','ChangePhone','DownloadOffer','RecalculateMedal','ParseBankTransactions','EnterHMRC','YodleeSearchWords','ChangeAddress','ChangeCompany',
'SendDocuments','AddEditDirector','UploadFile','SendSMS','RecheckFraud','AddProperty','LandRegistry','ZooplaRecheck','AddFunds','PacnetRequests','UnderwriterDashboard')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@JuniorUnderwriter)

/* adding Sales role relations 
					existing:	with ChangePhone,CRM,OpenBug,SuspendBtn,HideBrokerClientDetails,ReturnBtn permissions
						 new:	with DownloadOffer,TrustPilot,UploadFile,SendSMS permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @Sales, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('ChangePhone','CRM','OpenBug','SuspendBtn','HideBrokerClientDetails','ReturnBtn','DownloadOffer','TrustPilot','UploadFile','SendSMS','UnderwriterDashboard')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@Sales)

/* adding SuperUser role existing relation with OldEditLoanDetails,EditLoanDetails permission*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @SuperUser, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('OldEditLoanDetails','EditLoanDetails')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@SuperUser)

/* adding Underwriter role relations 
					existing:	with EmailConfirmationButton,CustomerStatus,TestUser,CRM,NewCreditLineButton,CreditLineFields,ApproveReject,Escalate,
								RecheckMarketplaces,CheckBankAccount,RunCreditBureauChecks,SendingMessagesToClients,OpenBug,AddBankAccount,
								ChangeBankAccount,AddDebitCard,RerunningMarketplaces,RerunningCreditCheck,ChangePhone,RecheckPayPal,Rollover,
								MakePayment,LoanOptions,AvoidAutomaticDecision,SuspendBtn,ReturnBtn,DiscountPlan,DisableShop permissions
						 new:	with DownloadOffer,RecalculateMedal,BlockTakingLoan,CCIMark,FraudStatus,TrustPilot,ParseBankTransactions,EnterHMRC,
								YodleeSearchWords,YodleeRules,DebitCardCustomerSelection,ChangeAddress,ChangeCompany,SendDocuments,AddEditDirector,UploadFile,
								DeleteFile,SendSMS,RecheckFraud,AddProperty,LandRegistry,ZooplaRecheck,AddFraudUser,AddFunds,PacnetRequests permissions*/
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @Underwriter, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('EmailConfirmationButton','CustomerStatus','TestUser','CRM','NewCreditLineButton','CreditLineFields','ApproveReject',
'Escalate','RecheckMarketplaces','CheckBankAccount','RunCreditBureauChecks','SendingMessagesToClients','OpenBug','AddBankAccount',
'ChangeBankAccount','AddDebitCard','RerunningMarketplaces','RerunningCreditCheck','ChangePhone','RecheckPayPal','Rollover',
'MakePayment','LoanOptions','AvoidAutomaticDecision','SuspendBtn','ReturnBtn','DiscountPlan','DisableShop',
'DownloadOffer','RecalculateMedal','BlockTakingLoan','CCIMark','FraudStatus','TrustPilot','ParseBankTransactions','EnterHMRC',
'YodleeSearchWords','YodleeRules','DebitCardCustomerSelection','ChangeAddress','ChangeCompany','SendDocuments','AddEditDirector','UploadFile',
'DeleteFile','SendSMS','RecheckFraud','AddProperty','LandRegistry','ZooplaRecheck','AddFraudUser','AddFunds','PacnetRequests','UnderwriterDashboard')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@Underwriter)

--removing BrokerSales role relations to CustomerStatus,RunCreditBureauChecks,MakePayment,TestUser,SendingMessagesToClients,AddBankAccount,ChangeBankAccount,AddDebitCard permission
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'BrokerSales')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @BrokerSales AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('CustomerStatus','RunCreditBureauChecks','MakePayment','TestUser','SendingMessagesToClients','AddBankAccount','ChangeBankAccount','AddDebitCard'))
END

/* removing Collector role relations to EmailConfirmationButton,TestUser,SendingMessagesToClients,RunCreditBureauChecks permission
and relations to bateli,emma,rosb user */
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Collector')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @Collector AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('EmailConfirmationButton','TestUser','SendingMessagesToClients','RunCreditBureauChecks'))
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @Collector AND UserId IN (SELECT u.UserId  FROM Security_User u WHERE 
	u.UserName IN ('bateli','emma','rosb'))
END

--removing JuniorUnderwriter role relations to RecheckPayPal,TestUser,RerunningMarketplaces permission
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'JuniorUnderwriter')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @JuniorUnderwriter AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('RecheckPayPal','TestUser','RerunningMarketplaces'))
END

/* removing manager role relations to RerunningCreditCheck,PacnetManualButton,RecheckPayPal permission
and relations to galitg,masha,sofiad,songulo,tomerg,vitasd user */
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'manager')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @manager AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('RerunningCreditCheck','PacnetManualButton','RecheckPayPal'))
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @manager AND UserId IN (SELECT u.UserId  FROM Security_User u WHERE 
	u.UserName IN ('galitg','masha','sofiad','songulo','tomerg','vitasd'))
END

--removing Sales role relations to TestUser,CheckBankAccount,SendingMessagesToClients,AddBankAccount,ChangeBankAccount,AddDebitCard,CustomerStatus permission
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Sales')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @Sales AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('TestUser','CheckBankAccount','SendingMessagesToClients','AddBankAccount','ChangeBankAccount','AddDebitCard','CustomerStatus'))
END

/* removing Underwriter role relations to ResetPassword,FinishWizard permission
and relations to dinusanp,masha user */
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Underwriter')
BEGIN
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @Underwriter AND PermissionId IN (SELECT p.Id  FROM Security_Permission p WHERE 
	p.Name IN ('ResetPassword','FinishWizard'))
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @Underwriter AND UserId IN (SELECT u.UserId  FROM Security_User u WHERE 
	u.UserName IN ('dinusanp','masha'))
END

/* removing SuperUser role relation to assafb user */
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'SuperUser')
BEGIN
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @SuperUser AND UserId IN (SELECT u.UserId  FROM Security_User u WHERE 
	u.UserName = 'assafb')
END

/* removing CollectorManager role relation to vitasd user */
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'CollectorManager')
BEGIN
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @CollectorManager AND UserId IN (SELECT u.UserId  FROM Security_User u WHERE 
	u.UserName = 'vitasd')
END

/* adding Accounting role relation with bateli user*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@Accounting FROM Security_User u WHERE u.UserName = 'bateli'
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@Accounting)

/* adding BrokerSales role relations 
					existing:	with nicolac,robm,travism users
						 new:	with hanryc,normanc,priyaw users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@BrokerSales FROM Security_User u WHERE u.UserName IN ('nicolac','robm','travism','hanryc','normanc','priyaw')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@BrokerSales)

/* adding Collector role existing relations with alexcollector,catherineh,jamiem,louip,nataliem,russellb,sailishr users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@Collector FROM Security_User u WHERE u.UserName IN ('alexcollector','catherineh','jamiem','louip','nataliem','russellb','sailishr')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@Collector)

/* adding CollectorSenior role existing relation with russellb user*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@CollectorSenior FROM Security_User u WHERE u.UserName = 'russellb'
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@CollectorSenior)

/* adding Sales role relations  
					existing:	with andreav,clareh,inie,jackiew,rosb,sarahd,scotth users
						 new:	with sarahb user*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@Sales FROM Security_User u WHERE u.UserName IN ('andreav','clareh','inie','jackiew','rosb','sarahd','scotth','sarahb')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@Sales)

/* adding CustomerCare role new relation with Jaime user*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@CustomerCare FROM Security_User u WHERE u.UserName = 'jamiem'
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@CustomerCare)

/* adding manager role existing relations with alexbo,assafb,dora,elinar,igaell,lirang,mishas,romanp,sashaf,shlomim,stasd,stuartd,yarons users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@manager FROM Security_User u WHERE u.UserName IN ('alexbo','assafb','dora','elinar','igaell','lirang','mishas','romanp','sashaf','shlomim','stasd','stuartd','yarons')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@manager)

/* adding SuperUser role existing relations with alexbo,elinar,stasd users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@SuperUser FROM Security_User u WHERE u.UserName IN ('alexbo','elinar','stasd')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@SuperUser)

/* adding Underwriter role relations   
					existing:	with sofiad,tanyag users
						 new:	with songulo,galitg users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@Underwriter FROM Security_User u WHERE u.UserName IN ('sofiad','tanyag','songulo','galitg')
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@Underwriter)

/* adding SuperUser role existing relations with alexbo,elinar,stasd users*/
INSERT INTO Security_UserRoleRelation (UserId,RoleId)
SELECT u.UserId,@Legal FROM Security_User u WHERE u.UserName ='jason'
AND u.UserId NOT IN (SELECT ur.UserId FROM Security_UserRoleRelation ur WHERE RoleId=@Legal)

GO
