SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTraceNames') AND name = 'IgnoreInGroupStats')
BEGIN
	ALTER TABLE DecisionTraceNames DROP COLUMN TimestampCounter

	ALTER TABLE DecisionTraceNames ADD IgnoreInGroupStats BIT NULL

	ALTER TABLE DecisionTraceNames ADD TimestampCounter ROWVERSION
END