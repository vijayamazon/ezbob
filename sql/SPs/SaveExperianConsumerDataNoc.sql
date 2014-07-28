SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataNoc') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataNoc
GO

IF TYPE_ID('ExperianConsumerDataNocList') IS NOT NULL
	DROP TYPE ExperianConsumerDataNocList
GO

CREATE TYPE ExperianConsumerDataNocList AS TABLE (
	ExperianConsumerDataId BIGINT NULL,
	Reference NVARCHAR(255) NULL,
	TextLine NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataNoc
@Tbl ExperianConsumerDataNocList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataNoc (
		ExperianConsumerDataId,
		Reference,
		TextLine
	) SELECT
		ExperianConsumerDataId,
		Reference,
		TextLine
	FROM @Tbl
END
GO