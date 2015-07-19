IF OBJECT_ID('NL_LoanInterestFreeze') IS NOT NULL
BEGIN
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_NL_LoanInterestFreeze_NL_Loans') 
		ALTER TABLE [dbo].[NL_LoanInterestFreeze] DROP CONSTRAINT FK_NL_LoanInterestFreeze_NL_Loans  ;   		
	DROP TABLE [dbo].[NL_LoanInterestFreeze];
END;

IF object_id('LoanAgreementTemplateTypes') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_LoanAgreementTemplateTypes') 
		ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT FK_LoanAgreementTemplate_LoanAgreementTemplateTypes  ;   		
	DROP TABLE [dbo].[LoanAgreementTemplateTypes];
end;


--IF object_id('NL_OfferStatuses') IS NOT NULL DROP TABLE [dbo].[NL_OfferStatuses];
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_OfferStatuses') BEGIN
		ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT FK_NL_Offers_NL_OfferStatuses  ;  END; 	
IF object_id('NL_BlendedLoans') IS NOT NULL DROP TABLE  [dbo].[NL_BlendedLoans];
IF object_id('NL_BlendedOffers') IS NOT NULL DROP TABLE [dbo].[NL_BlendedOffers];

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_Security_User') BEGIN
		ALTER TABLE [dbo].[LoanOptions]  DROP CONSTRAINT FK_LoanOptions_Security_User; END ;
		
IF EXISTS (select object_id from sys.all_objects where type_desc = 'DEFAULT_CONSTRAINT' and name = 'DF__LoanOptio__Inser__4D80A7ED') BEGIN
		ALTER TABLE [dbo].[LoanOptions]  DROP CONSTRAINT DF__LoanOptio__Inser__4D80A7ED; END ;	


IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'UserID')
	ALTER TABLE [dbo].[LoanOptions] DROP COLUMN [UserID]  ; 
GO
IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'InsertDate')
	ALTER TABLE [dbo].[LoanOptions] DROP COLUMN [InsertDate]  ; 
GO
IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'IsActive')
	ALTER TABLE [dbo].[LoanOptions] DROP COLUMN [IsActive]  ; 
GO
IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'Comments')
	ALTER TABLE [dbo].[LoanOptions] DROP COLUMN [Comments]  ; 
GO
IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'NLLoanID')
	ALTER TABLE [dbo].[LoanOptions] DROP COLUMN [NLLoanID]  ; 
GO

IF object_id('NL_CashRequests') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_NL_CashRequests') BEGIN
		ALTER TABLE [dbo].[MedalCalculationsAV] DROP CONSTRAINT [FK_MedalCalculationsAV_NL_CashRequests]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_NL_CashRequests') BEGIN
		ALTER TABLE [dbo].[MedalCalculations] DROP CONSTRAINT [FK_MedalCalculations_NL_CashRequests]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_CashRequests') BEGIN
		ALTER TABLE [dbo].[DecisionTrail] DROP CONSTRAINT [FK_DecisionTrail_NL_CashRequests]  ;  END;		 
	IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLCashRequestID')  BEGIN
		ALTER TABLE [dbo].[DecisionTrail] DROP COLUMN [NLCashRequestID]  ;  END;	
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_NL_CashRequests') BEGIN
		ALTER TABLE [dbo].[NL_Decisions]  DROP CONSTRAINT FK_NL_Decisions_NL_CashRequests; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_CashRequestOrigins') BEGIN
		ALTER TABLE [dbo].[NL_CashRequests] DROP CONSTRAINT [FK_NL_CashRequests_CashRequestOrigins] ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Customers') BEGIN
		ALTER TABLE [dbo].[NL_CashRequests] DROP CONSTRAINT [FK_NL_CashRequests_Customers] ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Users') BEGIN
		ALTER TABLE [dbo].[NL_CashRequests] DROP CONSTRAINT [FK_NL_CashRequests_Users] ;END;
	
	DROP TABLE  [dbo].[NL_CashRequests];	
end;

IF object_id('NL_CashRequestOrigins') IS NOT NULL begin DROP TABLE  [dbo].[NL_CashRequestOrigins]; end;
	
IF object_id('NL_DecisionRejectReasons') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DecisionRejectReasons_NL_Decision') BEGIN
		ALTER TABLE [dbo].[NL_DecisionRejectReasons]  DROP CONSTRAINT FK_NL_DecisionRejectReasons_NL_Decision;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DecisionRejectReasons_RejectReasons') BEGIN	
		ALTER TABLE [dbo].[NL_DecisionRejectReasons]  DROP CONSTRAINT FK_NL_DecisionRejectReasons_RejectReasons; END; 	
	DROP TABLE  [dbo].[NL_DecisionRejectReasons];
end;

IF object_id('NL_Decisions') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_NL_CashRequests') BEGIN
		ALTER TABLE [dbo].[NL_Decisions]  DROP CONSTRAINT FK_NL_Decisions_NL_CashRequests; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_DecisionNames') BEGIN 
		ALTER TABLE [dbo].[NL_Decisions]  DROP CONSTRAINT FK_NL_Decisions_DecisionNames  ;END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_Users') BEGIN
		ALTER TABLE [dbo].[NL_Decisions]  DROP CONSTRAINT FK_NL_Decisions_Users ; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_Decisions') BEGIN
		ALTER TABLE [dbo].[DecisionTrail]  DROP CONSTRAINT FK_DecisionTrail_NL_Decisions ; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Esignatures_NL_Decisions') BEGIN
		ALTER TABLE [dbo].[Esignatures] DROP CONSTRAINT FK_Esignatures_NL_Decisions  ;  END; 		
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DecisionRejectReasons_RejectReasons') BEGIN
		ALTER TABLE [dbo].[NL_DecisionRejectReasons] DROP CONSTRAINT [FK_NL_DecisionRejectReasons_RejectReasons]  ;  END; 		
	 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_Decisions') BEGIN
		ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT FK_NL_Offers_NL_Decisions  ;  END; 		
	DROP TABLE  [dbo].[NL_Decisions];
end;


IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_Decisions') BEGIN
	ALTER TABLE [dbo].[DecisionTrail]  DROP CONSTRAINT FK_DecisionTrail_NL_Decisions; END ;
IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID') BEGIN
	ALTER TABLE [dbo].[DecisionTrail]  DROP COLUMN NLDecisionID; END ;


IF object_id('NL_DiscountPlans') IS NOT NULL begin
IF object_id('NL_DiscountPlanEntries') IS NOT NULL DROP TABLE [dbo].[NL_DiscountPlanEntries];
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_DiscountPlans') BEGIN
		ALTER TABLE [dbo].NL_Offers DROP CONSTRAINT FK_NL_Offers_NL_DiscountPlans ; END; 
DROP TABLE [dbo].[NL_DiscountPlans];
end;

IF object_id('NL_EzbobBankAccounts') IS NOT NULL begin 	
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_EzbobBankAccounts') BEGIN
		ALTER TABLE [dbo].[NL_Loans] DROP CONSTRAINT FK_Loans_EzbobBankAccounts ; END; 
	DROP TABLE [dbo].[NL_EzbobBankAccounts];
end;
IF object_id('NL_FundTransfers') IS NOT NULL  begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PacnetTransactions_FundTransfers') BEGIN
		ALTER TABLE [dbo].[NL_PacnetTransactions] DROP CONSTRAINT [FK_PacnetTransactions_FundTransfers] ; END;  
	DROP TABLE NL_FundTransfers;
end;
IF object_id('NL_LoanAgreements') IS NOT NULL DROP TABLE NL_LoanAgreements;
IF object_id('NL_LoanFeePaymentHistory') IS NOT NULL 	DROP TABLE NL_LoanFeePaymentHistory;
IF object_id('NL_LoanFeePayments') IS NOT NULL 	DROP TABLE NL_LoanFeePayments;
IF object_id('NL_LoanFees') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanFees') BEGIN
		ALTER TABLE [dbo].[NL_LoanFeePayments] DROP CONSTRAINT [FK_LoanFeePayments_LoanFees] ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFees_LoanFeeTypes') BEGIN
		ALTER TABLE [dbo].[NL_LoanFees] DROP CONSTRAINT [FK_LoanFees_LoanFeeTypes] ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_AssignUser') BEGIN
		ALTER TABLE [dbo].[NL_LoanFees] DROP CONSTRAINT [FK_NL_LoanFees_AssignUser] ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_DeleteUser') BEGIN
		ALTER TABLE [dbo].[NL_LoanFees] DROP CONSTRAINT [FK_NL_LoanFees_DeleteUser] ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanFees') BEGIN
		ALTER TABLE [dbo].[NL_LoanRollovers] DROP CONSTRAINT [FK_NL_LoanRollovers_NL_LoanFees] ; END;			
	DROP TABLE NL_LoanFees;	
	end;
IF object_id('NL_LoanFeeTypes') IS NOT NULL DROP TABLE NL_LoanFeeTypes;
IF object_id('NL_CollectionLog') IS NOT NULL DROP TABLE NL_CollectionLog;
IF object_id('NL_LoanHistorySchedules') IS NOT NULL DROP TABLE NL_LoanHistorySchedules;
IF object_id('NL_LoanInterestFreeze') IS NOT NULL DROP TABLE NL_LoanInterestFreeze;
IF object_id('NL_LoanLegals') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_LoanLegals') BEGIN
		ALTER TABLE [dbo].[NL_LoanHistory] DROP CONSTRAINT [FK_NL_LoanHistory_NL_LoanLegals] ; END;	 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLegals_NL_Offers') BEGIN
		ALTER TABLE [dbo].[NL_LoanLegals] DROP CONSTRAINT [FK_NL_LoanLegals_NL_Offers] ; END;		
DROP TABLE NL_LoanLegals;
end;
IF object_id('NL_LoanLienLinks') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_NL_Loans') BEGIN
		ALTER TABLE [dbo].[NL_LoanLienLinks] DROP CONSTRAINT FK_NL_LoanLienLink_NL_Loans ; END; 
	DROP TABLE NL_LoanLienLinks;
end;
IF object_id('NL_LoanOptions') IS NOT NULL BEGIN
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_NL_Loans') BEGIN
		ALTER TABLE [dbo].[NL_LoanOptions] DROP CONSTRAINT FK_LoanOptions_NL_Loans ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_NL_Loans') BEGIN
		ALTER TABLE [dbo].[NL_LoanOptions] DROP CONSTRAINT [FK_LoanOptions_NL_Loans] ;END;
	DROP TABLE NL_LoanOptions;
END;
IF object_id('NL_LoanRollovers') IS NOT NULL DROP TABLE NL_LoanRollovers;	
IF object_id('NL_LoanSchedules') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanSchedules_LoanHistory') BEGIN
		ALTER TABLE [dbo].[NL_LoanSchedules] DROP CONSTRAINT [FK_NL_LoanSchedules_LoanHistory] ;END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanSchedules') BEGIN
		ALTER TABLE [dbo].[NL_LoanSchedulePayments] DROP CONSTRAINT [FK_LoanScheduleTransaction_LoanSchedules] ;END;  
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanSchedules_NL_LoanScheduleStatuses') BEGIN
		ALTER TABLE [dbo].[NL_LoanSchedules] DROP CONSTRAINT [FK_NL_LoanSchedules_NL_LoanScheduleStatuses] ;END; 
	DROP TABLE NL_LoanSchedules;
end;
IF object_id('NL_LoanScheduleStatuses') IS NOT NULL DROP TABLE NL_LoanScheduleStatuses;
IF object_id('NL_LoanSchedulePayment') IS NOT NULL DROP TABLE NL_LoanSchedulePayment;
IF object_id('NL_LoanSchedulePayments') IS NOT NULL DROP TABLE NL_LoanSchedulePayments;
IF object_id('NL_LoanSources') IS NOT NULL DROP TABLE NL_LoanSources;
IF object_id('NL_LoanStatuses') IS NOT NULL	begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_LoanStatuses') BEGIN
		ALTER TABLE [dbo].[NL_Loans] DROP CONSTRAINT [FK_Loans_LoanStatuses] ;END; 	   
	DROP TABLE NL_LoanStatuses;
end;
IF object_id('NL_PaymentStatuses') IS NOT NULL	begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_NL_PaymentStatuses') BEGIN
		ALTER TABLE [dbo].NL_Payments DROP CONSTRAINT FK_NL_Payments_NL_PaymentStatuses ; END; 
	DROP TABLE NL_PaymentStatuses;
end;

IF object_id('NL_Offers') IS NOT NULL begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLegals_NL_Offers') BEGIN
		ALTER TABLE [dbo].[NL_LoanLegals]  DROP CONSTRAINT [FK_NL_LoanLegals_NL_Offers]  ;END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanType') BEGIN
	 ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT [FK_NL_Offers_LoanType]  ;  END ;
	 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_Decisions') BEGIN
	 ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT [FK_NL_Offers_NL_Decisions] ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanSource') BEGIN
	 ALTER TABLE [dbo].[NL_Offers] DROP  CONSTRAINT FK_NL_Offers_LoanSource  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_DiscountPlans') BEGIN
	 ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT [FK_NL_Offers_NL_DiscountPlans]  ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_RepaymentIntervalType') BEGIN
		ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT [FK_NL_Offers_NL_RepaymentIntervalType]  ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_OfferStatuses') BEGIN
		ALTER TABLE [dbo].[NL_Offers] DROP CONSTRAINT FK_NL_Offers_NL_OfferStatuses  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedOffers_NL_Offers') BEGIN
		ALTER TABLE [dbo].[NL_BlendedOffers] DROP CONSTRAINT [FK_NL_BlendedOffers_NL_Offers]  ;  END; 
			
	DROP TABLE NL_Offers;
end;
IF object_id('NL_PacnetTransactions') IS NOT NULL	DROP TABLE NL_PacnetTransactions;
IF object_id('NL_PacnetTransactionStatuses') IS NOT NULL	DROP TABLE NL_PacnetTransactionStatuses;
IF object_id('NL_PaymentMethods') IS NOT NULL	DROP TABLE NL_PaymentMethods;
IF object_id('NL_Payments') IS NOT NULL	begin
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Payments_PaymentTypes') BEGIN
		ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT [FK_Payments_PaymentTypes] ;END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Payments_CreatedBySecurity_User') BEGIN
		ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT FK_Payments_CreatedBySecurity_User  ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_DeletedBySecurity_User') BEGIN
		ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT FK_NL_Payments_DeletedBySecurity_User  ;END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_LoanTransactionMethod') BEGIN
		ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT [FK_NL_Payments_LoanTransactionMethod] ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_NL_PaymentStatuses') BEGIN
		ALTER TABLE [dbo].NL_Payments DROP CONSTRAINT FK_NL_Payments_NL_PaymentStatuses ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PaypointTransactions_Payments') BEGIN
		ALTER TABLE [dbo].[NL_PaypointTransactions] DROP CONSTRAINT [FK_PaypointTransactions_Payments] ; END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_LoanTransactionMethod') BEGIN
		ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT [FK_NL_Payments_LoanTransactionMethod] ; END; 	

	DROP TABLE NL_Payments;
end;
IF object_id('NL_PaypointTransactions') IS NOT NULL	BEGIN
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_PayPointCard') BEGIN
		ALTER TABLE [dbo].[NL_PaypointTransactions] DROP CONSTRAINT [FK_NL_PaypointTransactions_PayPointCard]  ;  END; 
	DROP TABLE NL_PaypointTransactions;
END;
IF object_id('NL_PaypointTransactionStatuses') IS NOT NULL	DROP TABLE NL_PaypointTransactionStatuses;
IF object_id('NL_RepaymentIntervalTypes') IS NOT NULL begin 
	 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_RepaymentIntervalTypes') BEGIN
		ALTER TABLE [dbo].[NL_Loans] DROP CONSTRAINT [FK_NL_Loans_NL_RepaymentIntervalTypes]  ;  END;	
	DROP TABLE NL_RepaymentIntervalTypes;
	end;

	
IF object_id('NL_LoanHistory') IS NOT NULL BEGIN
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_CollectionLog_NL_LoanHistory') BEGIN
		ALTER TABLE [dbo].[CollectionLog] DROP CONSTRAINT [FK_CollectionLog_NL_LoanHistory]  ; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_NL_LoanHistory') BEGIN
		ALTER TABLE [dbo].[LoanOptions] DROP CONSTRAINT [FK_LoanOptions_NL_LoanHistory]  ; END ;
	DROP TABLE NL_LoanHistory;
END;
IF object_id('NL_LoanStates') IS NOT NULL DROP TABLE NL_LoanStates;
IF object_id('NL_Loans') IS NOT NULL begin	
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan') BEGIN
		ALTER TABLE [dbo].[LoanBrokerCommission] DROP CONSTRAINT [FK_LoanBrokerCommission_NL_Loan]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_LoanSources') BEGIN
		ALTER TABLE [dbo].[NL_Loans] DROP CONSTRAINT [FK_Loans_LoanSources]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_LoanStatuses') BEGIN
		ALTER TABLE [dbo].[NL_Loans]  DROP CONSTRAINT [FK_Loans_LoanStatuses]  ;  END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_EzbobBankAccounts') BEGIN
		ALTER TABLE [dbo].[NL_Loans]  DROP CONSTRAINT [FK_Loans_EzbobBankAccounts]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_LoanType') BEGIN
		ALTER TABLE [dbo].[NL_Loans]  DROP CONSTRAINT [FK_NL_Loans_LoanType]  ;  END; 
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_Offers') BEGIN
		ALTER TABLE [dbo].[NL_Loans]  DROP CONSTRAINT [FK_NL_Loans_NL_Offers] ; END;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_RepaymentIntervalTypes') BEGIN
		ALTER TABLE [dbo].[NL_Loans] DROP  CONSTRAINT [FK_NL_Loans_NL_RepaymentIntervalTypes]  ;END;	
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedLoans_NL_Loans') BEGIN
		ALTER TABLE [dbo].[NL_BlendedLoans] DROP CONSTRAINT FK_NL_BlendedLoans_NL_Loans  ;END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedOffers_NL_Offers') BEGIN
		ALTER TABLE [dbo].[NL_BlendedOffers] DROP CONSTRAINT [FK_NL_BlendedOffers_NL_Offers]  ; END ;
	IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan') BEGIN
		ALTER TABLE [dbo].[LoanBrokerCommission] DROP CONSTRAINT [FK_LoanBrokerCommission_NL_Loan]  ; END ;
		
	DROP TABLE [dbo].[NL_Loans];	
end;
IF object_id('WriteOffReasons') IS NOT NULL DROP TABLE WriteOffReasons;


IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID') begin
ALTER TABLE [dbo].[LoanAgreementTemplate] DROP CONSTRAINT [DF_TemplateTypeID];
ALTER TABLE [dbo].[LoanAgreementTemplate] DROP COLUMN TemplateTypeID;
end;


IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreementTemplate_LoanAgreementTemplateTypes')BEGIN
ALTER TABLE LoanAgreementTemplate DROP CONSTRAINT FK_NL_LoanAgreementTemplate_LoanAgreementTemplateTypes;
end;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_FundTransfers_Loans') BEGIN
ALTER TABLE [dbo].[NL_FundTransfers] DROP CONSTRAINT [FK_FundTransfers_Loans]  ;
END ;
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] DROP CONSTRAINT FK_NL_LoanAgreements_LoanHistory; END; 

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanAgreementTemplate') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] DROP CONSTRAINT FK_NL_LoanAgreements_LoanAgreementTemplate; END;
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePaymentHistory_LoanFees') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePaymentHistory] DROP CONSTRAINT FK_LoanFeePaymentHistory_LoanFees ; END;
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePaymentHistory_LoanTransactions') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePaymentHistory] DROP CONSTRAINT [FK_LoanFeePaymentHistory_LoanTransactions] ; END;
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePaymentHistory_LoanTransactions')  BEGIN
ALTER TABLE [dbo].[NL_LoanFeePaymentHistory] DROP CONSTRAINT [FK_LoanFeePaymentHistory_LoanTransactions]  ; END;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFees_LoanFeeTypes') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] DROP CONSTRAINT [FK_LoanFees_LoanFeeTypes]; END; 

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_NL_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] DROP CONSTRAINT [FK_NL_LoanFees_NL_LoanHistory] ; END;

 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanScheduleHistory_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] DROP CONSTRAINT [FK_NL_LoanScheduleHistory_Security_User] ; END;
 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] DROP CONSTRAINT [FK_NL_LoanHistory_NL_Loans]  ;END;
 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_LoanLegals') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] DROP CONSTRAINT FK_NL_LoanHistory_NL_LoanLegals ;END; 
	

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistorySchedule_NL_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanHistorySchedules] DROP CONSTRAINT [FK_NL_LoanHistorySchedule_NL_LoanHistory]  ;END;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistorySchedule_NL_LoanSchedules') BEGIN
ALTER TABLE [dbo].[NL_LoanHistorySchedules] DROP CONSTRAINT [FK_NL_LoanHistorySchedule_NL_LoanSchedules]  ;END;



IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_LoanLien') BEGIN
ALTER TABLE [dbo].[NL_LoanLienLinks] DROP CONSTRAINT [FK_NL_LoanLienLink_LoanLien]  ;END;


 
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] DROP CONSTRAINT [FK_NL_LoanRollovers_Security_User] ; END;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User1') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers]  DROP CONSTRAINT [FK_NL_LoanRollovers_Security_User1]  ; END;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanFees') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] DROP CONSTRAINT [FK_NL_LoanRollovers_NL_LoanFees]  ; END;
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanHistory') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers]  DROP CONSTRAINT [FK_NL_LoanRollovers_NL_LoanHistory]  ;END;

 IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanSchedules') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayment] DROP  CONSTRAINT [FK_LoanScheduleTransaction_LoanSchedules]  ; END;

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanTransactions') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayment] DROP CONSTRAINT [FK_LoanScheduleTransaction_LoanTransactions] ; END;



IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] DROP CONSTRAINT [FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses]  ; END; 
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PacnetTransactions_FundTransfers') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] DROP CONSTRAINT [FK_PacnetTransactions_FundTransfers]  ; END; 



IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] DROP CONSTRAINT [FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses] ; END;





IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DiscountPlanEntries_NL_DiscountPlans') BEGIN
 ALTER TABLE [dbo].[NL_DiscountPlanEntries] DROP CONSTRAINT [FK_NL_DiscountPlanEntries_NL_DiscountPlans] ; END; 

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[DecisionTrail] DROP CONSTRAINT FK_DecisionTrail_NL_Decisions ; END; 

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan') BEGIN
 ALTER TABLE [dbo].[LoanBrokerCommission] DROP CONSTRAINT [FK_LoanBrokerCommission_NL_Loan]  ;  END; 
	
IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculations] DROP CONSTRAINT [FK_MedalCalculations_Medals] ;	 END; 

IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV] DROP CONSTRAINT [FK_MedalCalculationsAV_Medals] ; END;


IF EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_NL_Loans') BEGIN
 ALTER TABLE [dbo].[LoanOptions] DROP CONSTRAINT [FK_LoanOptions_NL_Loans] ;
END;

-- Enable all the constraint in database
--EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";