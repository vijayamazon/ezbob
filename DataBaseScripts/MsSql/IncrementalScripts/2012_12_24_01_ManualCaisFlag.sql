go
ALTER TABLE [dbo].[CaisFlags] ALTER COLUMN FlagSetting nvarCHAR(20);
go
ALTER TABLE [dbo].[LoanOptions] ALTER COLUMN ManualCaisFlag nvarCHAR(20);

go
update [dbo].[CaisFlags]
	set [FlagSetting] ='Calculated value'
where [FlagSetting] = 'CV'

go
update [dbo].[LoanOptions]
	set [ManualCaisFlag] ='Calculated value'
where [ManualCaisFlag] = 'CV'