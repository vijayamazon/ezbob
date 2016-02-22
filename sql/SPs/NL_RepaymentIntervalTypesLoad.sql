SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_RepaymentIntervalTypesLoad') IS NULL
	EXECUTE('CREATE PROCEDURE NL_RepaymentIntervalTypesLoad AS SELECT 1')
GO

ALTER PROCEDURE NL_RepaymentIntervalTypesLoad
AS
BEGIN
	SET NOCOUNT ON;
   
	SELECT
		RepaymentIntervalTypeID,
		IsMonthly,
		LengthInDays,
		Description
	FROM
		NL_RepaymentIntervalTypes
END
GO
