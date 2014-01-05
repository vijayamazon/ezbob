IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetLateLoanStatusGetConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetLateLoanStatusGetConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetLateLoanStatusGetConfigs] 
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod1') AS CollectionPeriod1,
		(SELECT id FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod1') AS CollectionPeriod1Id,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod2') AS CollectionPeriod2,
		(SELECT id FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod2') AS CollectionPeriod21Id,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod3') AS CollectionPeriod3,
		(SELECT id FROM ConfigurationVariables cv WHERE cv.Name = 'CollectionPeriod3') AS CollectionPeriod3Id,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'LatePaymentCharge') AS LatePaymentCharge,
		(SELECT id FROM ConfigurationVariables cv WHERE cv.Name = 'LatePaymentCharge') AS LatePaymentChargeId,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'PartialPaymentCharge') AS PartialPaymentCharge,
		(SELECT Id FROM ConfigurationVariables cv WHERE cv.Name = 'PartialPaymentCharge') AS PartialPaymentChargeId,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'AdministrationCharge') AS AdministrationCharge,
		(SELECT Id FROM ConfigurationVariables cv WHERE cv.Name = 'AdministrationCharge') AS AdministrationChargeId,
		(SELECT Value FROM ConfigurationVariables cv WHERE cv.Name = 'AmountToChargeFrom') AS AmountToChargeFrom
END
GO
