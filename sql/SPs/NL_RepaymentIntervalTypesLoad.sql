SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_RepaymentIntervalTypesLoad') IS NOT NULL
	DROP PROCEDURE NL_RepaymentIntervalTypesLoad;
GO

CREATE PROCEDURE [dbo].[NL_RepaymentIntervalTypesLoad]
AS
BEGIN
	
	SET NOCOUNT ON;
   
	SELECT  [RepaymentIntervalTypeID], [RepaymentIntervalType]  FROM [dbo].[NL_RepaymentIntervalTypes];
END
