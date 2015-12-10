SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveNewUnmatchedPaymentTimestamps') IS NOT NULL
	DROP PROCEDURE SaveNewUnmatchedPaymentTimestamps
GO

IF TYPE_ID('IntTimestampList') IS NOT NULL
	DROP TYPE IntTimestampList
GO

CREATE TYPE IntTimestampList AS TABLE (
	ItemID INT,
	TimestampCounter BINARY(8)
)
GO

CREATE PROCEDURE SaveNewUnmatchedPaymentTimestamps
@TimestampList IntTimestampList READONLY
AS
BEGIN
	MERGE
		ReportedUnmatchedPayments p -- this is target
	USING
		@TimestampList l -- this is source
	ON
		p.PaymentID = l.ItemID
	WHEN MATCHED THEN -- found in both source and target => update target from source
		UPDATE SET
			p.LastKnownTimestamp = l.TimestampCounter
	WHEN NOT MATCHED BY TARGET THEN -- found in source but not in target => insert into target
		INSERT (PaymentID, LastKnownTimestamp) VALUES (l.ItemID, l.TimestampCounter)
	;
END
GO
