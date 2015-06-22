SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanFeeTypesLoad') IS NOT NULL
	DROP PROCEDURE NL_LoanFeeTypesLoad;
GO

CREATE PROCEDURE [dbo].[NL_LoanFeeTypesLoad]
AS
BEGIN	
	SET NOCOUNT ON;
   
	SELECT LoanFeeTypeID, LoanFeeType from NL_LoanFeeTypes;
END
