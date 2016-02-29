SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM LogicalGlueEtlCodes WHERE CommunicationCode = 'E')
	INSERT INTO LogicalGlueEtlCodes(EtlCode, CommunicationCode, IsHardReject, IsError) VALUES ('Parsing error', 'E', 0, 1)
GO
