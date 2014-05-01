IF OBJECT_ID('UpdateExperianConsumer') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateExperianConsumer AS SELECT 1')
GO

ALTER PROCEDURE UpdateExperianConsumer
@Name NVARCHAR(500),
@Surname NVARCHAR(500),
@PostCode NVARCHAR(500),
@ExperianError NVARCHAR(max),
@ExperianScore INT, 
@CustomerID BIGINT,
@DirectorID BIGINT,
@BirthDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE MP_ExperianDataCache SET
		ExperianError = @ExperianError, 
		ExperianScore = @ExperianScore, 
		CustomerId    = @CustomerID,
		DirectorId    = @DirectorID 
	WHERE
		Name      = @Name
		AND
		Surname   = @Surname
		AND
		PostCode  = @PostCode
		AND
		BirthDate = @BirthDate
END
GO
