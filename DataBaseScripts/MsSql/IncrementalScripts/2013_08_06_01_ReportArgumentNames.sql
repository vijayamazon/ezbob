IF OBJECT_ID('ReportArgumentNames') IS NULL
BEGIN
	DECLARE @DateRange NVARCHAR(64) = 'DateRange'

	CREATE TABLE ReportArgumentNames (
		Id INT NOT NULL,
		Name NVARCHAR(64) NOT NULL,
		CONSTRAINT PK_ReportArgumentNames PRIMARY KEY (Id)
	)

	CREATE UNIQUE INDEX IDX_ReportArgumentName ON ReportArgumentNames(Name)
	
	INSERT INTO ReportArgumentNames (Id, Name) VALUES
		(1, @DateRange),
		(2, 'Customer'),
		(3, 'ShowNonCashTransactions')

	CREATE TABLE ReportArguments (
		Id INT IDENTITY(1, 1) NOT NULL,
		ReportArgumentNameId INT NOT NULL,
		ReportId INT NOT NULL,
		CONSTRAINT FK_ReportArgument_Name FOREIGN KEY (ReportArgumentNameId) REFERENCES ReportArgumentNames (Id),
		CONSTRAINT FK_ReportArgument_Report FOREIGN KEY (ReportId) REFERENCES ReportScheduler (Id)
	)

	CREATE UNIQUE INDEX IDX_ReportArgument ON ReportArguments(ReportArgumentNameId, ReportId)

	INSERT INTO ReportArguments(ReportArgumentNameId, ReportId)
	SELECT
		ran.Id,
		r.Id
	FROM
		ReportScheduler r
		INNER JOIN ReportArgumentNames ran ON ran.Name = @DateRange
END
GO
