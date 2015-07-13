IF OBJECT_ID('GetLoanTypeAndDefault') IS NULL 
	EXECUTE('CREATE PROCEDURE GetLoanTypeAndDefault AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLoanTypeAndDefault
@LoanTypeID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @OutputLoanTypeID INT = NULL
	DECLARE @DefaultLoanTypeID INT = NULL

	IF EXISTS (SELECT * FROM LoanType WHERE Id = @LoanTypeID)
		SET @OutputLoanTypeID = @LoanTypeID

	SELECT TOP 1
		@DefaultLoanTypeID = Id
	FROM
		LoanType
	WHERE
		IsDefault = 1

	IF @DefaultLoanTypeID IS NULL
		SELECT @DefaultLoanTypeId = Id FROM LoanType WHERE Type = 'StandardLoanType'

	SELECT
		LoanTypeID = @OutputLoanTypeID,
		DefaultLoanTypeID = @DefaultLoanTypeID
END
GO
