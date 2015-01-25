IF OBJECT_ID('LoadCustomerCompanyNames') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerCompanyNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerCompanyNames
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------
	--
	-- Start of output
	--
	------------------------------------------------------------------------------

	EXECUTE LoadHmrcBusinessNames @CustomerID, @Now, 1

	------------------------------------------------------------------------------

	SELECT
		RowType             = 'CompanyName',
		ExperianCompanyName = co.ExperianCompanyName,
		EnteredCompanyName  = co.CompanyName,
		FullName            = c.FullName
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'ExperianName',
		e.Forename,
		e.MiddleName,
		e.Surname
	FROM
		ExperianConsumerDataApplicant e
	WHERE
		e.ExperianConsumerDataId = @ExperianConsumerDataID
END
GO
