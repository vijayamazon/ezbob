IF OBJECT_ID('GetMainStrategyStallerData') IS NULL
	EXECUTE('CREATE PROCEDURE GetMainStrategyStallerData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMainStrategyStallerData
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT		
		ExperianRefNum,
		CAST(CASE WHEN LastStartedMainStrategyEndTime IS NULL THEN 0 ELSE 1 END AS BIT) AS MainStrategyExecutedBefore,
		Company.TypeOfBusiness,
		Name AS CustomerEmail
	FROM
		Customer
		INNER JOIN Company ON Customer.CompanyId = Company.Id
	WHERE
		Customer.Id = @CustomerId
END
GO
