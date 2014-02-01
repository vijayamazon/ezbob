IF OBJECT_ID('QuickOfferConfiguration') IS NULL
BEGIN
	CREATE TABLE QuickOfferConfiguration (
		ID                     INT            NOT NULL CONSTRAINT DF_QOffCfg_ID                     DEFAULT (1),
		Enabled                BIT            NOT NULL CONSTRAINT DF_QOffCfg_Enabled                DEFAULT (1),
		CompanySeniorityMonths INT            NOT NULL CONSTRAINT DF_QOffCfg_CompanySeniorityMonths DEFAULT (36),
		ApplicantMinAgeYears   INT            NOT NULL CONSTRAINT DF_QOffCfg_ApplicantMinAgeYears   DEFAULT (18),
		NoDefaultsInLastMonths INT            NOT NULL CONSTRAINT DF_QOffCfg_NoDefaultsInLastMonths DEFAULT (24),
		AmlMin                 INT            NOT NULL CONSTRAINT DF_QOffCfg_AmlMin                 DEFAULT (71),
		PersonalScoreMin       INT            NOT NULL CONSTRAINT DF_QOffCfg_PersonalScoreMin       DEFAULT (560),
		BusinessScoreMin       INT            NOT NULL CONSTRAINT DF_QOffCfg_BusinessScoreMin       DEFAULT (31),
		MaxLoanCountPerDay     INT            NOT NULL CONSTRAINT DF_QOffCfg_MaxLoanCountPerDay     DEFAULT (5),
		MaxIssuedValuePerDay   INT            NOT NULL CONSTRAINT DF_QOffCfg_MaxIssuedValuePerDay   DEFAULT (35000),
		OfferDurationHours     INT            NOT NULL CONSTRAINT DF_QOffCfg_OfferDurationHours     DEFAULT (24),
		MinOfferAmount         INT            NOT NULL CONSTRAINT DF_QOffCfg_MinOfferAmount         DEFAULT (1000),
		OfferCapPct            DECIMAL(12, 6) NOT NULL CONSTRAINT DF_QOffCfg_OfferCapPct            DEFAULT (0.15),
		ImmediateMaxAmount     INT            NOT NULL CONSTRAINT DF_QOffCfg_ImmediateMaxAmount     DEFAULT (7000),
		ImmediateTermMonths    INT            NOT NULL CONSTRAINT DF_QOffCfg_ImmediateTermMonths    DEFAULT (3),
		ImmediateInterestRate  DECIMAL(12, 6) NOT NULL CONSTRAINT DF_QOffCfg_ImmediateInterestRate  DEFAULT (0.035),
		ImmediateSetupFee      DECIMAL(12, 6) NOT NULL CONSTRAINT DF_QOffCfg_ImmediateSetupFee      DEFAULT (0.015),
		PotentialMaxAmount     INT            NOT NULL CONSTRAINT DF_QOffCfg_PotentialMaxAmount     DEFAULT (50000),
		PotentialTermMonths    INT            NOT NULL CONSTRAINT DF_QOffCfg_PotentialTermMonths    DEFAULT (12),
		PotentialSetupFee      DECIMAL(12, 6) NOT NULL CONSTRAINT DF_QOffCfg_PotentialSetupFee      DEFAULT (0.015),
		OfferAmountCalculator  NTEXT          NOT NULL CONSTRAINT DF_QOffCfg_OfferAmountCalculator  DEFAULT ('{ "40": 0.012, "50": 0.016, "60": 0.027, "70": 0.031, "80": 0.034, "90": 0.051, "100": 0.069 }'),
		PriceCalculator        NTEXT          NOT NULL CONSTRAINT DF_QOffCfg_PriceCalculator        DEFAULT ('{ "40": { "15000": 0.0508250073035349,"20000": 0.0413907878079657,"25000": 0.0366736780601811,"30000": 0.0338434122115104,"35000": 0.0319565683123965,"40000": 0.0306088226701724,"45000": 0.0295980134385042,"50000": 0.0288118284805401,"100000": 0.0281828805141689}, "50": { "15000": 0.0482969790066564,"20000": 0.0389301928656767,"25000": 0.0342467997951869,"30000": 0.031436763952893,"35000": 0.029563406724697,"40000": 0.0282252944188428,"45000": 0.0272217101894521,"50000": 0.0264411446777038,"100000": 0.0258166922683052}, "60": { "15000": 0.04506104468219,"20000": 0.0357805747849801,"25000": 0.0311403398363751,"30000": 0.0283561988672121,"35000": 0.0265001048877701,"40000": 0.0251743234738829,"45000": 0.0241799874134676,"50000": 0.0234066149220334,"100000": 0.0227879169288861}, "70": { "15000": 0.0417932159165036,"20000": 0.0325999130245706,"25000": 0.028003261578604,"30000": 0.0252452707110241,"35000": 0.0234066101326375,"40000": 0.0220932811480757,"45000": 0.0211082844096543,"50000": 0.0203421758353265,"100000": 0.0197292889758643}, "80": { "15000": 0.0393389656938044,"20000": 0.0302111281788701,"25000": 0.025647209421403,"30000": 0.0229088581669227,"35000": 0.0210832906639358,"40000": 0.02,"45000": 0.02,"50000": 0.02,"100000": 0.02}, "90": { "15000": 0.0367770770496298,"20000": 0.0277175760899369,"25000": 0.0231878256100905,"30000": 0.0204699753221826,"35000": 0.02,"40000": 0.02,"45000": 0.02,"50000": 0.02,"100000": 0.02}, "100": { "15000": 0.0333966817496229,"20000": 0.0244273504273504,"25000": 0.0199426847662142,"30000": 0.02,"35000": 0.02,"40000": 0.02,"45000": 0.02,"50000": 0.02,"100000": 0.02} }'),
		TimestampCounter       ROWVERSION,
		CONSTRAINT PK_QuickOfferConfiguration PRIMARY KEY (ID),
		CONSTRAINT CHK_QuickOfferConfiguration CHECK (ID = 1),

		CONSTRAINT CHK_QOffCfg_CompanySeniorityMonths CHECK (CompanySeniorityMonths >= 0),
		CONSTRAINT CHK_QOffCfg_ApplicantMinAgeYears   CHECK (ApplicantMinAgeYears >= 18),
		CONSTRAINT CHK_QOffCfg_NoDefaultsInLastMonths CHECK (NoDefaultsInLastMonths >= 0),
		CONSTRAINT CHK_QOffCfg_AmlMin                 CHECK (AmlMin >= 0),
		CONSTRAINT CHK_QOffCfg_PersonalScoreMin       CHECK (PersonalScoreMin >= 0),
		CONSTRAINT CHK_QOffCfg_BusinessScoreMin       CHECK (BusinessScoreMin >= 0),
		CONSTRAINT CHK_QOffCfg_MaxLoanCountPerDay     CHECK (MaxLoanCountPerDay >= 0),
		CONSTRAINT CHK_QOffCfg_MaxIssuedValuePerDay   CHECK (MaxIssuedValuePerDay >= 0),
		CONSTRAINT CHK_QOffCfg_OfferDurationHours     CHECK (OfferDurationHours >= 0),
		CONSTRAINT CHK_QOffCfg_MinOfferAmount         CHECK (MinOfferAmount >= 0),
		CONSTRAINT CHK_QOffCfg_OfferCapPct            CHECK (OfferCapPct >= 0),
		CONSTRAINT CHK_QOffCfg_ImmediateMaxAmount     CHECK (ImmediateMaxAmount >= 0),
		CONSTRAINT CHK_QOffCfg_ImmediateTermMonths    CHECK (ImmediateTermMonths >= 0),
		CONSTRAINT CHK_QOffCfg_ImmediateInterestRate  CHECK (ImmediateInterestRate >= 0),
		CONSTRAINT CHK_QOffCfg_ImmediateSetupFee      CHECK (ImmediateSetupFee >= 0),
		CONSTRAINT CHK_QOffCfg_PotentialMaxAmount     CHECK (PotentialMaxAmount >= 0),
		CONSTRAINT CHK_QOffCfg_PotentialTermMonths    CHECK (PotentialTermMonths >= 0),
		CONSTRAINT CHK_QOffCfg_PotentialSetupFee      CHECK (PotentialSetupFee >= 0)
	)

	INSERT INTO QuickOfferConfiguration (ID) VALUES (1)
END
GO

IF OBJECT_ID('QuickOfferLoadConfiguration') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferLoadConfiguration AS SELECT 1')
GO

ALTER PROCEDURE QuickOfferLoadConfiguration
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM QuickOfferConfiguration WHERE ID = 1)
		INSERT INTO QuickOfferConfiguration (ID) VALUES (1)

	SELECT
		Enabled,
		CompanySeniorityMonths,
		ApplicantMinAgeYears,
		NoDefaultsInLastMonths,
		AmlMin,
		PersonalScoreMin,
		BusinessScoreMin,
		MaxLoanCountPerDay,
		MaxIssuedValuePerDay,
		OfferDurationHours,
		MinOfferAmount,
		OfferCapPct,
		ImmediateMaxAmount,
		ImmediateTermMonths,
		ImmediateInterestRate,
		ImmediateSetupFee,
		PotentialMaxAmount,
		PotentialTermMonths,
		PotentialSetupFee,
		OfferAmountCalculator,
		PriceCalculator
	FROM
		QuickOfferConfiguration
	WHERE
		ID = 1
END
GO

DELETE FROM ConfigurationVariables WHERE Name = 'QuickOfferDurationHours'
GO

IF OBJECT_ID('dbo.udfQuickOfferDuration') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfQuickOfferDuration() RETURNS INT AS BEGIN RETURN 24 END')
GO

ALTER FUNCTION dbo.udfQuickOfferDuration()
RETURNS INT
AS
BEGIN
	DECLARE @qodh INT

	SELECT
		@qodh = OfferDurationHours
	FROM
		QuickOfferConfiguration
	WHERE
		ID = 1

	RETURN @qodh
END
GO
