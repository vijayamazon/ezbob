IF OBJECT_ID('[GetSlidersData]') IS NULL
	EXECUTE('CREATE PROCEDURE [GetSlidersData] AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetSlidersData]
	@CustomerID INT
AS
BEGIN	
	DECLARE @OriginID INT = (SELECT OriginID FROM Customer WHERE Id=@CustomerID)
	DECLARE @DefaultLoanSourceID DECIMAL = (SELECT LoanSourceID FROM DefaultLoanSources WHERE OriginID = @OriginID)
	DECLARE @Amount DECIMAL
	DECLARE @Term INT
	DECLARE @HasLoan BIT = (SELECT count(*) FROM Loan WHERE CustomerId = @CustomerID) 

	SELECT TOP 1 @Amount= Amount, @Term = Term FROM CustomerRequestedLoan WHERE CustomerID=@CustomerID ORDER BY Created DESC

	SET NOCOUNT ON
	SELECT
		ISNULL(@Amount,-1) AS Amount,
		ISNULL(@Term,-1) AS Term,
		igr.MinLoanAmount AS MinLoanAmount,
		igr.MaxLoanAmount AS MaxLoanAmount,
		igr.MinTerm AS MinTerm,
		igr.MaxTerm AS MaxTerm
	FROM
		I_GradeRange igr
	WHERE 
		igr.OriginID = @OriginID
	AND 
		igr.LoanSourceID=@DefaultLoanSourceID 
	AND 
		igr.GradeID IS NULL
	AND
		igr.IsFirstLoan <> @HasLoan 
	AND
		igr.IsActive = 1
END
GO

