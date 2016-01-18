delete from I_InvestorRule
delete from I_Parameter
delete from I_InvestorParams

select * from I_InvestorRule
select * from I_Parameter
select * from I_InvestorParams
select * from I_Investor
select * from CashRequests
select * from I_ProductSubType
select * from I_InvestorBankAccount where InvestorID = 1
select * from I_InvestorSystemBalance
select * from I_Portfolio where InvestorID = 1
select * from Loan where id = 1
select * from I_Index where InvestorID = 1


update I_Investor set MonthlyFundingCapital = 500000 

INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,null,null,2,3,1,1)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot)VALUES  (1,1,null,null,null,null,4,5,1,0)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,null,null,6,7,1,0)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','DailyAvailableAmount',null,null,3,0)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','WeeklyAvailableAmount',null,null,3,0)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,null,'ManagerApprovedSum','Balance',null,null,3,0)
INSERT INTO I_InvestorRule(UserID,RuleType,InvestorID,FuncName,MemberNameSource,MemberNameTarget,LeftParamID,RightParamID,Operator,IsRoot) VALUES (1,1,null,'RuleBadgetLevel',null,null,null,null,7,0)

INSERT INTO I_Parameter(Name,ValueType,DefaultValue,MaxLimit,MinLimit) VALUES ('DailyInvestmentAllowed', 'double', 0, null, null)
INSERT INTO I_Parameter(Name,ValueType,DefaultValue,MaxLimit,MinLimit) VALUES('WeeklyInvestmentAllowed', 'double', 0, null , null)

INSERT INTO I_InvestorParams(InvestorID,ParameterID,Value,Type,AllowedForConfig) VALUES(null, 1, 1000000, 1,1)
INSERT INTO I_InvestorParams(InvestorID,ParameterID,Value,Type,AllowedForConfig) VALUES(null, 2, 3000000, 1,1)

INSERT INTO I_Portfolio(InvestorID,ProductTypeID,LoanID,LoanPercentage,InitialTerm,GradeID,Timestamp) VALUES (1,1,1,1,'',1,GETDATE())
INSERT INTO dbo.I_InvestorBankAccount(InvestorID,InvestorAccountTypeID,BankName,BankCode,BankCountryID,BankBranchName,BankBranchNumber,BankAccountName,BankAccountNumber,RepaymentKey,IsActive,UserID,Timestamp) VALUES  (1,1,1,'',1,'',1,'',1,1,1,1,getdate())
INSERT INTO dbo.I_InvestorSystemBalance(InvestorBankAccountID,PreviousBalance,NewBalance,TransactionAmount,ServicingFeeAmount,Timestamp,CashRequestID,LoanID,LoanTransactionID,Comment) VALUES(1,100,200,5000,1,getdate(),1,1,1,'')




