IF OBJECT_ID('GetWorstCaisStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE GetWorstCaisStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetWorstCaisStatuses
@CustomerId INT
AS
BEGIN
	SELECT rtrim(ltrim(WorstStatus)) AS WorstStatus FROM ExperianConsumerData WHERE WorstStatus IS NOT NULL AND CustomerId = @CustomerId
END
GO
