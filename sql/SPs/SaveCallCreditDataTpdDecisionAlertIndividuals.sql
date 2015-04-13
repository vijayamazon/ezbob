SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdDecisionAlertIndividuals') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdDecisionAlertIndividuals
GO

IF TYPE_ID('CallCreditDataTpdDecisionAlertIndividualsList') IS NOT NULL
	DROP TYPE CallCreditDataTpdDecisionAlertIndividualsList
GO

CREATE TYPE CallCreditDataTpdDecisionAlertIndividualsList AS TABLE (
	[CallCreditDataTpdID] BIGINT NULL,
	[IndividualName] NVARCHAR(164) NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpdDecisionAlertIndividuals
@Tbl CallCreditDataTpdDecisionAlertIndividualsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataTpdDecisionAlertIndividuals (
		[CallCreditDataTpdID],
		[IndividualName]
	) SELECT
		[CallCreditDataTpdID],
		[IndividualName]
	FROM @Tbl
END
GO


