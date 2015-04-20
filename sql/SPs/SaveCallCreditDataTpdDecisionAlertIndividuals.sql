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

	DECLARE @CallCreditDataTpdDecisionAlertIndividualsId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataTpdDecisionAlertIndividuals table.', 11, 1)

	INSERT INTO CallCreditDataTpdDecisionAlertIndividuals (
		[CallCreditDataTpdID],
		[IndividualName]
	) SELECT
		[CallCreditDataTpdID],
		[IndividualName]
	FROM @Tbl

	SET @CallCreditDataTpdDecisionAlertIndividualsId = SCOPE_IDENTITY()

	SELECT @CallCreditDataTpdDecisionAlertIndividualsId AS CallCreditDataTpdDecisionAlertIndividualsId
END
GO


