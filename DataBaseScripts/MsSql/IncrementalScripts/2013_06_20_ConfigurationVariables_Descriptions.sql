update [ConfigurationVariables] set [Description] = 'if Enabled BWA Experian check will be performed on Business accounts' where [Name] = 'BWABusinessCheck'
update [ConfigurationVariables] set [Description] = 'if Enabled customer earned points is displayed in sign up wizard and customer dashboard' where [Name] = 'DisplayEarnedPoints'
update [ConfigurationVariables] set [Description] = 'if Enabled system will reject customers automatically without any Underwriter actions' where [Name] = 'EnableAutomaticRejection'
update [ConfigurationVariables] set [Description] = 'if Enabled system will Approve customers automatically without any Underwriter actions' where [Name] = 'EnableAutomaticApproval'
update [ConfigurationVariables] set [Description] = 'The lowest Score that Customer needs to have in order not to be rejected' where [Name] = 'LowCreditScore'
update [ConfigurationVariables] set [Description] = 'The lowest Annual Turnover that Customer needs to have in order not to be rejected' where [Name] = 'TotalAnnualTurnover'
update [ConfigurationVariables] set [Description] = 'The lowest 3 Months Turnover that Customer needs to have in order not to be rejected' where [Name] = 'TotalThreeMonthTurnover'
update [ConfigurationVariables] set [Description] = '% of O/S balance. The charge applied when passed to collection. NOTE: this charge can be only modified from code, the value in this table doesn`t affect our system.' where [Name] = 'CollectionsCharge'
go
