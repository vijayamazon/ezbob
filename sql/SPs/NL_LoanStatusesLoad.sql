SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_LoanStatusesLoad;
GO

CREATE PROCEDURE [dbo].[NL_LoanStatusesLoad]
AS
BEGIN

	SET NOCOUNT ON;

	SELECT [LoanStatusID], [LoanStatus], [TimestampCounter] FROM [dbo].[NL_LoanStatuses];
END
