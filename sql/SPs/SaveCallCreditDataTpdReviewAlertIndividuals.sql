SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdReviewAlertIndividuals') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdReviewAlertIndividuals
GO

IF TYPE_ID('CallCreditDataTpdReviewAlertIndividualsList') IS NOT NULL
	DROP TYPE CallCreditDataTpdReviewAlertIndividualsList
GO

CREATE TYPE CallCreditDataTpdReviewAlertIndividualsList AS TABLE (
	[CallCreditDataTpdID] BIGINT NULL,
	[IndividualName] NVARCHAR(164) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpdReviewAlertIndividuals
@Tbl CallCreditDataTpdReviewAlertIndividualsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataTpdReviewAlertIndividuals (
		[CallCreditDataTpdID],
		[IndividualName]
	) SELECT
		[CallCreditDataTpdID],
		[IndividualName]
	FROM @Tbl
END
GO


