SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadGradeRangeAndSubproduct') IS NULL
	EXECUTE('CREATE PROCEDURE LoadGradeRangeAndSubproduct AS SELECT 1')
GO

ALTER PROCEDURE LoadGradeRangeAndSubproduct
@GradeRangeID INT,
@ProductSubTypeID INT
AS
BEGIN
	SELECT
		r.MinSetupFee,
		r.MaxSetupFee,
		r.MinInterestRate,
		r.MaxInterestRate,
		r.MinLoanAmount,
		r.MaxLoanAmount,
		r.MinTerm,
		r.MaxTerm,
		st.LoanSourceID,
		lt.LoanTypeID,
		r.TurnoverShare,
		r.ValueAddedShare,
		r.FreeCashFlowShare
	FROM
		I_GradeRange r
		INNER JOIN I_ProductSubType st ON st.ProductSubTypeID = @ProductSubTypeID
		CROSS APPLY dbo.udfGetLoanType(0) lt
	WHERE
		r.GradeRangeID = @GradeRangeID
END
GO
