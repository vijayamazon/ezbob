delete from ConfigurationVariables where name = 'EnableAutomaticApproval'
delete from ConfigurationVariables where name = 'LowCreditScore'
delete from ConfigurationVariables where name = 'TotalAnnualTurnover'
delete from ConfigurationVariables where name = 'TotalThreeMonthTurnover'
GO

INSERT INTO ConfigurationVariables (Name, [Value], [Description])
VALUES ('EnableAutomaticApproval', '1', 'Auto re-approval')
INSERT INTO ConfigurationVariables (Name, [Value], [Description])
VALUES ('LowCreditScore', '500', 'Low credit score')
INSERT INTO ConfigurationVariables (Name, [Value], [Description])
VALUES ('TotalAnnualTurnover', '7000', 'Total annual turnover')
INSERT INTO ConfigurationVariables (Name, [Value], [Description])
VALUES ('TotalThreeMonthTurnover', '1500', 'Total 3-month turnover')
GO