IF OBJECT_ID('GetExperianDirectorsNamesForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianDirectorsNamesForCustomer AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianDirectorsNamesForCustomer
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RefNum NVARCHAR(50)
	DECLARE @CompanyID INT
	DECLARE @ExperianLtdID BIGINT

	------------------------------------------------------------------------------

	SELECT 
		@RefNum = co.ExperianRefNum,
		@CompanyID = co.Id
	FROM
		Customer c
		INNER JOIN Company co ON c.CompanyId = co.Id

	------------------------------------------------------------------------------

	DECLARE @SrvLogID BIGINT = (
		SELECT TOP 1
			l.Id
		FROM
			MP_ServiceLog l
		WHERE (
				l.CompanyRefNum = @RefNum
				OR
				l.CompanyID = @CompanyID
			)
			AND
			l.ServiceType = 'E-SeriesLimitedData'
		ORDER BY
			l.InsertDate DESC
	)

	------------------------------------------------------------------------------

	SELECT
		@ExperianLtdID = l.ExperianLtdID
	FROM
		ExperianLtd l
	WHERE
		l.ServiceLogID = @SrvLogID

	------------------------------------------------------------------------------

	SELECT
		d.FirstName,
		d.LastName,
		RowType = 'DirectorName'
	FROM
		ExperianLtdDL72 d
	WHERE
		d.ExperianLtdID = @ExperianLtdID

	UNION

	SELECT
		d.FirstName,
		d.LastName,
		RowType = 'DirectorName'
	FROM
		ExperianLtdDLB5 d
	WHERE
		d.ExperianLtdID = @ExperianLtdID
END
GO
