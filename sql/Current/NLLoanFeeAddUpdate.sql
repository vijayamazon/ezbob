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

IF EXISTS (select id from sysobjects where name='NL_LoansFeesGet') and NOT EXISTS (select id from sysobjects where name='NL_LoanFeesGet')
BEGIN
	EXEC sp_rename 'NL_LoansFeesGet', 'NL_LoanFeesGet';
	drop procedure NL_LoansFeesGet;
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