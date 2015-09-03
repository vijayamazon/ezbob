SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('CustomerAndDirectorsServiceLogIDs') IS NOT NULL
	DROP TYPE CustomerAndDirectorsServiceLogIDs
GO

CREATE TYPE CustomerAndDirectorsServiceLogIDs AS TABLE (
	CustomerID INT NULL,
	DirectorID INT NULL,
	ServiceLogID BIGINT NULL,
	ExperianConsumerDataID BIGINT NULL
)
GO
