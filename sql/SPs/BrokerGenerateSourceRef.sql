IF OBJECT_ID('BrokerGenerateSourceRef') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerGenerateSourceRef AS SELECT 1')
GO

ALTER PROCEDURE BrokerGenerateSourceRef
@BrokerID INT,
@SourceRef NVARCHAR(10) OUTPUT
AS
BEGIN
	DECLARE @Value INT = @BrokerID

	DECLARE @TARGET_BASE INT = 26
	DECLARE @FullLen INT = 10

	DECLARE @Fill NVARCHAR(10) = ''
	DECLARE @Buf NVARCHAR(10) = ''
	DECLARE @Head NVARCHAR(10) = 'brk-'

	DECLARE @Missing INT

	WHILE (1 = 1)
	BEGIN
		SET @Buf = CHAR(ASCII('a') + @Value % @TARGET_BASE) + @Buf
		SET @Value = @Value / @TARGET_BASE
	 	IF NOT (@Value > 0) BREAK
	END

	SET @Missing = @FullLen - LEN(@Head) - LEN(@Buf)

	WHILE LEN(@Fill) < @Missing
	BEGIN
		SET @Fill = @Fill + CHAR(ASCII('a') + ABS(CHECKSUM(NEWID())) % 26)
	END

	SET @SourceRef = @Head + @Fill + @Buf
END
GO
