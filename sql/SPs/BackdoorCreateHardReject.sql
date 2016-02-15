IF OBJECT_ID('BackdoorCreateHardReject') IS NULL
	EXECUTE('CREATE PROCEDURE BackdoorCreateHardReject AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BackdoorCreateHardReject
@CustomerId INT,
@MonthlyRepayment INT,
@UniqueID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	DECLARE @ServiceLogID BIGINT
	DECLARE @RequestID BIGINT
	DECLARE @ResponseID BIGINT
	DECLARE @ModelOutputID BIGINT
	DECLARE @EtlDataID BIGINT
	DECLARE @HistoryEntryID BIGINT
	DECLARE @ErrorMsg NVARCHAR(1024) = NULL

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	BEGIN TRY
		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		DECLARE @EtlCodeID BIGINT = (SELECT EtlCodeID FROM LogicalGlueEtlCodes WHERE IsHardReject = 1)

		-------------------------------------------------------------------------

		IF @EtlCodeID IS NULL
			RAISERROR('Failed to find Hard Reject ETL code', 11, 3)

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		INSERT INTO MP_ServiceLog (ServiceType, InsertDate, RequestData, ResponseData, CustomerId, CompanyID, Firstname, Surname, DateOfBirth, Postcode)
		SELECT
			'LogicalGlue',
			@Now,
			'backdoor',
			'backdoor',
			c.Id,
			c.CompanyId,
			c.FirstName,
			c.Surname,
			c.DateOfBirth,
			ISNULL(a.Rawpostcode, a.Postcode)
		FROM
			Customer c
			INNER JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType = 1
		WHERE
			c.Id = @CustomerID

		-------------------------------------------------------------------------

		SET @ServiceLogID = SCOPE_IDENTITY()

		-------------------------------------------------------------------------

		IF @ServiceLogID IS NULL
			RAISERROR('Failed to create MP_ServiceLog entry', 11, 4)

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		INSERT INTO LogicalGlueRequests (ServiceLogID, IsTryOut, UniqueID, MonthlyRepayment, EquifaxData, HouseNumber)
			VALUES (@ServiceLogID, 0, @UniqueID, @MonthlyRepayment, NULL, '0')

		-------------------------------------------------------------------------

		SET @RequestID = SCOPE_IDENTITY()

		-------------------------------------------------------------------------

		IF @RequestID IS NULL
			RAISERROR('Failed to create Request entry', 11, 5)

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		INSERT INTO LogicalGlueResponses (ServiceLogID, ReceivedTime, HttpStatus, ResponseStatus, GradeID, HasEquifaxData)
			VALUES (@ServiceLogID, @Now, 200, 200, NULL, 0)

		-------------------------------------------------------------------------

		SET @ResponseID = SCOPE_IDENTITY()

		-------------------------------------------------------------------------

		IF @ResponseID IS NULL
			RAISERROR('Failed to create Response entry', 11, 6)

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		INSERT INTO LogicalGlueEtlData (ResponseID, EtlCodeID)
			VALUES (@ResponseID, @EtlCodeID)

		-------------------------------------------------------------------------

		SET @EtlDataID = SCOPE_IDENTITY()

		-------------------------------------------------------------------------

		IF @EtlDataID IS NULL
			RAISERROR('Failed to create ETL data entry', 11, 8)

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		INSERT INTO CustomerLogicalGlueHistory (CustomerID, CompanyID, ResponseID, IsActive, GradeID, Score, IsHardReject, ScoreIsReliable, ErrorInResponse, SetTime)
		SELECT
			c.Id,
			c.CompanyId,
			@ResponseID,
			1,
			NULL,
			NULL,
			1,
			1,
			0,
			@Now
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID

		-------------------------------------------------------------------------

		SET @HistoryEntryID = SCOPE_IDENTITY()

		-------------------------------------------------------------------------

		IF @HistoryEntryID IS NULL
			RAISERROR('Failed to create history entry', 11, 9)

		-------------------------------------------------------------------------

		UPDATE CustomerLogicalGlueHistory SET
			IsActive = 0
		FROM
			CustomerLogicalGlueHistory h
			INNER JOIN Customer c
				ON h.CustomerID = c.Id
				AND h.CompanyID = c.CompanyID
				AND h.CustomerID = @CustomerID
				AND h.EntryID != @HistoryEntryID

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION

		SET @ErrorMsg = dbo.udfGetErrorMsg()
	END CATCH

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION

	IF @ErrorMsg IS NOT NULL
		RAISERROR('Failed to insert backdoor LG hard reject: %s', 11, 0, @ErrorMsg)
END
GO
