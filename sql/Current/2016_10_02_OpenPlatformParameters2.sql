SET QUOTED_IDENTIFIER ON
GO
if not exists (select * from ConfigurationVariables where name = 'InvestorBudgetAmplitude')
begin
insert into ConfigurationVariables (Name, Value, Description, IsEncrypted ) values ('InvestorBudgetAmplitude', '0.12', 'Investor Budget Amplitude', NULL)
end
GO
