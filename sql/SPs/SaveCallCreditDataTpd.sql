SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpd') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpd
GO

IF TYPE_ID('CallCreditDataTpdList') IS NOT NULL
	DROP TYPE CallCreditDataTpdList
GO

CREATE TYPE CallCreditDataTpdList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[TotalD] INT NULL,
	[TotalR] INT NULL,
	[Total36mJudgmesntsD] INT NULL,
	[TotalJudgmesntsD] INT NULL,
	[TotalActiveAmountJudgmesntsD] INT NULL,
	[CurrentlyInsolventD] BIT NULL,
	[RestrictedD] BIT NULL,
	[WorsePayStatus12mD] NVARCHAR(2) NULL,
	[WorsePayStatus24mD] NVARCHAR(2) NULL,
	[TotalDefaultsD] INT NULL,
	[TotalDefaults12mD] INT NULL,
	[TotalSettledDefaultsD] INT NULL,
	[TotalDefaultsAmountD] INT NULL,
	[TotalWriteoffsD] INT NULL,
	[TotalWriteoffsAmountD] INT NULL,
	[TotalDelinqsD] INT NULL,
	[TotalDelinqsAmountD] INT NULL,
	[Total36mJudgmesntsR] INT NULL,
	[TotalJudgmesntsR] INT NULL,
	[TotalActiveAmountJudgmesntsR] INT NULL,
	[CurrentlyInsolventR] BIT NULL,
	[RestrictedR] BIT NULL,
	[WorsePayStatus12mR] NVARCHAR(2) NULL,
	[WorsePayStatus24mR] NVARCHAR(2) NULL,
	[TotalDefaultsR] INT NULL,
	[TotalDefaults12mR] INT NULL,
	[TotalSettledDefaultsR] INT NULL,
	[TotalDefaultsAmountR] INT NULL,
	[TotalWriteoffsR] INT NULL,
	[TotalWriteoffsAmountR] INT NULL,
	[TotalDelinqsR] INT NULL,
	[TotalDelinqsAmountR] INT NULL,
	[ThinFile] BIT NULL,
	[TotalH] INT NULL,
	[Total36mJudgmentsH] INT NULL,
	[TotalJudgmentsH] INT NULL,
	[TotalSatisfiedJudgmesntsH] INT NULL,
	[TotalActiveAmountJudgmesntsH] INT NULL,
	[TotalSatisfiedAmountJudgmesntsH] INT NULL,
	[CurrentlyInsolventH] BIT NULL,
	[RestrictedH] BIT NULL,
	[TotalAccountsH] INT NULL,
	[TotalActiveAccountsH] INT NULL,
	[TotalActiveAccountsAmountH] INT NULL,
	[TotalAccountsZerobalH] INT NULL,
	[TotalSettledAccountsAmountH] INT NULL,
	[WorsePayStatus12mH] NVARCHAR(2) NULL,
	[WorsePayStatus24mH] NVARCHAR(2) NULL,
	[TotalDefaultsH] INT NULL,
	[TotalDefaults12mH] INT NULL,
	[TotalDefaultsAmountH] INT NULL,
	[TotalWriteoffsH] INT NULL,
	[TotalWriteoffsAmountH] INT NULL,
	[TotalDelinqsH] INT NULL,
	[TotalDelinqsAmountH] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpd
@Tbl CallCreditDataTpdList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CallCreditDataTpdId BIGINT
	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataTpd table.', 11, 1)

	INSERT INTO CallCreditDataTpd (
		[CallCreditDataID],
		[TotalD],
		[TotalR],
		[Total36mJudgmesntsD],
		[TotalJudgmesntsD],
		[TotalActiveAmountJudgmesntsD],
		[CurrentlyInsolventD],
		[RestrictedD],
		[WorsePayStatus12mD],
		[WorsePayStatus24mD],
		[TotalDefaultsD],
		[TotalDefaults12mD],
		[TotalSettledDefaultsD],
		[TotalDefaultsAmountD],
		[TotalWriteoffsD],
		[TotalWriteoffsAmountD],
		[TotalDelinqsD],
		[TotalDelinqsAmountD],
		[Total36mJudgmesntsR],
		[TotalJudgmesntsR],
		[TotalActiveAmountJudgmesntsR],
		[CurrentlyInsolventR],
		[RestrictedR],
		[WorsePayStatus12mR],
		[WorsePayStatus24mR],
		[TotalDefaultsR],
		[TotalDefaults12mR],
		[TotalSettledDefaultsR],
		[TotalDefaultsAmountR],
		[TotalWriteoffsR],
		[TotalWriteoffsAmountR],
		[TotalDelinqsR],
		[TotalDelinqsAmountR],
		[ThinFile],
		[TotalH],
		[Total36mJudgmentsH],
		[TotalJudgmentsH],
		[TotalSatisfiedJudgmesntsH],
		[TotalActiveAmountJudgmesntsH],
		[TotalSatisfiedAmountJudgmesntsH],
		[CurrentlyInsolventH],
		[RestrictedH],
		[TotalAccountsH],
		[TotalActiveAccountsH],
		[TotalActiveAccountsAmountH],
		[TotalAccountsZerobalH],
		[TotalSettledAccountsAmountH],
		[WorsePayStatus12mH],
		[WorsePayStatus24mH],
		[TotalDefaultsH],
		[TotalDefaults12mH],
		[TotalDefaultsAmountH],
		[TotalWriteoffsH],
		[TotalWriteoffsAmountH],
		[TotalDelinqsH],
		[TotalDelinqsAmountH]
	) SELECT
		[CallCreditDataID],
		[TotalD],
		[TotalR],
		[Total36mJudgmesntsD],
		[TotalJudgmesntsD],
		[TotalActiveAmountJudgmesntsD],
		[CurrentlyInsolventD],
		[RestrictedD],
		[WorsePayStatus12mD],
		[WorsePayStatus24mD],
		[TotalDefaultsD],
		[TotalDefaults12mD],
		[TotalSettledDefaultsD],
		[TotalDefaultsAmountD],
		[TotalWriteoffsD],
		[TotalWriteoffsAmountD],
		[TotalDelinqsD],
		[TotalDelinqsAmountD],
		[Total36mJudgmesntsR],
		[TotalJudgmesntsR],
		[TotalActiveAmountJudgmesntsR],
		[CurrentlyInsolventR],
		[RestrictedR],
		[WorsePayStatus12mR],
		[WorsePayStatus24mR],
		[TotalDefaultsR],
		[TotalDefaults12mR],
		[TotalSettledDefaultsR],
		[TotalDefaultsAmountR],
		[TotalWriteoffsR],
		[TotalWriteoffsAmountR],
		[TotalDelinqsR],
		[TotalDelinqsAmountR],
		[ThinFile],
		[TotalH],
		[Total36mJudgmentsH],
		[TotalJudgmentsH],
		[TotalSatisfiedJudgmesntsH],
		[TotalActiveAmountJudgmesntsH],
		[TotalSatisfiedAmountJudgmesntsH],
		[CurrentlyInsolventH],
		[RestrictedH],
		[TotalAccountsH],
		[TotalActiveAccountsH],
		[TotalActiveAccountsAmountH],
		[TotalAccountsZerobalH],
		[TotalSettledAccountsAmountH],
		[WorsePayStatus12mH],
		[WorsePayStatus24mH],
		[TotalDefaultsH],
		[TotalDefaults12mH],
		[TotalDefaultsAmountH],
		[TotalWriteoffsH],
		[TotalWriteoffsAmountH],
		[TotalDelinqsH],
		[TotalDelinqsAmountH]
	FROM @Tbl

	SET @CallCreditDataTpdId = SCOPE_IDENTITY()

	SELECT @CallCreditDataTpdId AS CallCreditDataTpdId
END
GO


