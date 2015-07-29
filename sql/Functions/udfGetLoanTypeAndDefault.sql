IF OBJECT_ID('dbo.udfGetLoanTypeAndDefault') IS NOT NULL
	DROP FUNCTION dbo.udfGetLoanTypeAndDefault
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetLoanTypeAndDefault(@LoanTypeID INT)
RETURNS @output TABLE (LoanTypeID INT, DefaultLoanTypeID INT)
AS
BEGIN
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

	INSERT INTO @output(LoanTypeID, DefaultLoanTypeID)
		VALUES (@OutputLoanTypeID, @DefaultLoanTypeID)

	RETURN
END
GO
