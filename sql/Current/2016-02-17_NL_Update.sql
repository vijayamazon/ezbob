
DECLARE @lastid INT

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'Write Off')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'Write Off', 0)
END
ELSE
	update [LoanTransactionMethod]  set Id = 10 where [Name] = 'Write Off';

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'SetupFeeOffset')
 BEGIN
	 SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	 INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'SetupFeeOffset', 0)
END
ELSE
	update [LoanTransactionMethod]  set Id = 11 where [Name] = 'SetupFeeOffset';

 IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'SystemRepay')
 BEGIN
	 SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	 INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'SystemRepay', 0)
 END
 ELSE
	update [LoanTransactionMethod]  set Id = 12 where [Name] = 'SystemRepay';




IF NOT EXISTS(select LoanFeeTypeID from NL_LoanFeeTypes where LoanFeeType = 'OtherCharge')  
BEGIN  
	insert into NL_LoanFeeTypes ([LoanFeeTypeID],[LoanFeeType],[DefaultAmount],[Description]) select 8,Name,Value,[Description] from ConfigurationVariables cf where Name = 'OtherCharge';
END
  
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanFees') AND name = 'UpdatedByUserID') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanFees] ADD UpdatedByUserID int null; 
	ALTER TABLE [dbo].[NL_LoanFees] ADD CONSTRAINT [FK_NL_LoanFees_UpdateUser] FOREIGN KEY([AssignedByUserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
	ALTER TABLE [dbo].[NL_LoanFees] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanFees] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanFees') AND name = 'UpdateTime') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanFees] ADD [UpdateTime] datetime null; 
	ALTER TABLE [dbo].[NL_LoanFees] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanFees] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanFees') AND name = 'OldFeeID') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanFees] ADD [OldFeeID] int null; 
	ALTER TABLE [dbo].[NL_LoanFees] ADD CONSTRAINT [FK_NL_LoanFees_LoanCharges] FOREIGN KEY([OldFeeID]) REFERENCES [dbo].[LoanCharges] ([Id]) ON DELETE SET NULL ;
	ALTER TABLE [dbo].[NL_LoanFees] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanFees] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('NL_Offers') AND name='ProductSubTypeID')
BEGIN
	ALTER TABLE [dbo].[NL_Offers] ADD ProductSubTypeID INT ;
	ALTER TABLE [dbo].[NL_Offers] ADD CONSTRAINT FK_NL_Offers_I_ProductSubType FOREIGN KEY (ProductSubTypeID) REFERENCES I_ProductSubType (ProductSubTypeID);
	ALTER TABLE [dbo].[NL_Offers] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_Offers] ADD TimestampCounter timestamp not null; 
END
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanHistory') AND name = 'LateFees') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanHistory] ADD LateFees decimal(18,6) null ; 
	ALTER TABLE [dbo].[NL_LoanHistory] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanHistory] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanHistory') AND name = 'DistributedFees') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanHistory] ADD DistributedFees decimal(18,6) null ; 
	ALTER TABLE [dbo].[NL_LoanHistory] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanHistory] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('NL_LoanHistory') AND name = 'OutstandingInterest') 
BEGIN
	ALTER TABLE [dbo].[NL_LoanHistory] ADD OutstandingInterest decimal(18,6) null ; 
	ALTER TABLE [dbo].[NL_LoanHistory] drop column [TimestampCounter];
	ALTER TABLE [dbo].[NL_LoanHistory] ADD TimestampCounter timestamp not null; 
END

IF NOT EXISTS (SELECT id FROM sysobjects WHERE  name = 'DF_NL_LoanHistoryLateFees') 
BEGIN
	alter table [dbo].[NL_LoanHistory] add constraint DF_NL_LoanHistoryLateFees default 0 for [LateFees];
END
IF NOT EXISTS (SELECT id FROM sysobjects WHERE  name = 'DF_NL_LoanHistoryDistributedFees') 
BEGIN
	alter table [dbo].[NL_LoanHistory] add constraint DF_NL_LoanHistoryDistributedFees default 0 for DistributedFees;
END
IF NOT EXISTS (SELECT id FROM sysobjects WHERE  name = 'DF_NL_LoanHistoryOutstandingInterest') 
BEGIN
	alter table [dbo].[NL_LoanHistory] add constraint DF_NL_LoanHistoryOutstandingInterest default 0 for OutstandingInterest;
END






IF EXISTS (select id from sysobjects where name='NL_LoansFeesGet') and NOT EXISTS (select id from sysobjects where name='NL_LoanFeesGet')
BEGIN
	EXEC sp_rename 'NL_LoansFeesGet', 'NL_LoanFeesGet';
	-- drop procedure NL_LoansFeesGet;
END

IF EXISTS (select id from sysobjects where name='NL_LoanFeeDisable') and NOT EXISTS (select id from sysobjects where name='NL_LoanFeeDisable')
BEGIN
	drop procedure NL_LoanFeeDisable;
END

IF NOT EXISTS (SELECT [LoanScheduleStatusID] FROM [dbo].[NL_LoanScheduleStatuses] WHERE [LoanScheduleStatus] = 'LateDeletedOnReschedule') BEGIN 	
	insert into [dbo].[NL_LoanScheduleStatuses] ( [LoanScheduleStatusID], [LoanScheduleStatus], [Description]) values (6, 'LateDeletedOnReschedule', 'Was late on reschedule') ;
END

alter table [dbo].[NL_LoanInterestFreeze] alter column [StartDate] date;
alter table [dbo].[NL_LoanInterestFreeze] alter column [EndDate] date;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'UC_CRDecisionTime')
BEGIN
	alter table [dbo].[NL_Decisions] add CONSTRAINT UC_CRDecisionTime UNIQUE ([CashRequestID],[DecisionNameID],[DecisionTime]);
END

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'UC_OldCR')
BEGIN
	alter table [dbo].[NL_CashRequests] add CONSTRAINT UC_OldCR UNIQUE ([OldCashRequestID]);
END

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'UC_Desicion')
BEGIN
	alter table [dbo].[NL_Offers] add CONSTRAINT UC_Desicion UNIQUE ([DecisionID]);
END

IF EXISTS (select id from sysobjects where name='NL_LoanFeesOldIDUpdate') 
BEGIN
	EXEC NL_LoanFeesOldIDUpdate;
END

IF EXISTS (select id from sysobjects where name='UC_LoanFeePayments') 
BEGIN
	alter table [dbo].[NL_LoanFeePayments] drop CONSTRAINT UC_LoanFeePayments;
END

alter table [dbo].[NL_LoanFeePayments] add CONSTRAINT UC_LoanFeePayments UNIQUE (LoanFeeID, PaymentID, Amount, [ResetAmount]);


IF EXISTS (select id from sysobjects where name='CHK_NL_LoanSchedulePayments_Principal') 
BEGIN
	alter table [dbo].[NL_LoanSchedulePayments] drop CONSTRAINT CHK_NL_LoanSchedulePayments_Principal;
END
IF EXISTS (select id from sysobjects where name='CHK_NL_LoanSchedulePayments_Interest') 
BEGIN
	alter table [dbo].[NL_LoanSchedulePayments] drop CONSTRAINT CHK_NL_LoanSchedulePayments_Interest;
END


-- remove duplicates from [dbo].[NL_LoanSchedulePayments]
BEGIN
WITH ste_DuplicateSchedulePayments (lastRecord, [LoanScheduleID],[PaymentID],[PrincipalPaid],[InterestPaid],[ResetPrincipalPaid],[ResetInterestPaid])
AS(
	select max([LoanSchedulePaymentID]) as lastRecord, [LoanScheduleID],[PaymentID],[PrincipalPaid],[InterestPaid],[ResetPrincipalPaid],[ResetInterestPaid]  from [dbo].[NL_LoanSchedulePayments] 
	group by [LoanScheduleID],[PaymentID],[PrincipalPaid],[InterestPaid],[ResetPrincipalPaid],[ResetInterestPaid] having 
	 count([LoanSchedulePaymentID]) > 1  
	)
	--select * from [dbo].[NL_LoanSchedulePayments] where [LoanSchedulePaymentID] in (select lastRecord from ste_DuplicateSchedulePayments);
	delete from [dbo].[NL_LoanSchedulePayments] where [LoanSchedulePaymentID] in (select lastRecord from ste_DuplicateSchedulePayments);
END

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'UC_NL_LoanSchedulePayments')
BEGIN
	alter table [dbo].[NL_LoanSchedulePayments] add CONSTRAINT UC_NL_LoanSchedulePayments UNIQUE ([LoanScheduleID],[PaymentID],[PrincipalPaid],[InterestPaid],[ResetPrincipalPaid],[ResetInterestPaid]);
END