SET QUOTED_IDENTIFIER ON
GO
 
UPDATE ConfigurationVariables SET
 Value = '12'
WHERE
 Name = 'LatePaymentCharge' or name = 'AdministrationCharge'
 AND
 Value = '20'
GO