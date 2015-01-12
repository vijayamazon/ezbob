-- Dependant on SP dbo.GetMarketplaceFromHistoryID

IF OBJECT_ID('UpdateMpTotalsHmrc') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsHmrc AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[UpdateMpTotalsHmrc]
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MpID INT

	EXECUTE dbo.GetMarketplaceFromHistoryID 'HMRC', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN
		
	------------------------------------------------------------------------------
	-- 2. Select from/to dates and amounts for relevant periods and relevant boxes only.	
	------------------------------------------------------------------------------

	create table #order_items(
		RecordId int,
		DateFrom DATETIME,
		DateTo DATETIME,
		Amount numeric(18,2),
		boxNum int 
	)

	insert into #order_items (RecordId, DateFrom, DateTo, Amount, boxNum) 
	SELECT
		i.RecordId ,	-- ==MP_VatReturnRecord.Id
		o.DateFrom ,
		o.DateTo ,
		i.Amount ,
		(SELECT CASE 
				WHEN n.Name LIKE '%(Box 6)%' THEN 6
				WHEN n.Name LIKE '%(Box 7)%' THEN 7	
			END) AS boxNum
	FROM
		MP_VatReturnEntries i		
		INNER JOIN
			( SELECT 
				r.DateFrom,
				r.DateTo,
				MAX(r.Id) AS Id					
			FROM
				MP_VatReturnRecords r INNER JOIN Business b ON (r.BusinessId = b.Id AND b.BelongsToCustomer = 1 AND r.CustomerMarketPlaceId = @MpID)
				INNER JOIN dbo.MP_VatReturnRecordDeleteHistory delh on (delh.HistoryItemID = r.Id and ISNULL(r.IsDeleted, 0) = 0 )
				INNER JOIN dbo.MP_CustomerMarketPlaceUpdatingHistory h on (h.CustomerMarketPlaceId = r.CustomerMarketPlaceId and h.UpdatingEnd < delh.DeletedTime)	
			GROUP BY 
				r.DateFrom, r.DateTo
			)	as o on (o.Id  = i.RecordId and ISNULL(i.IsDeleted, 0) = 0 )
		INNER JOIN MP_VatReturnEntryNames n ON (n.Id = i.NameId AND (n.Name LIKE '%(Box 6)%' or n.Name LIKE '%(Box 7)%'))	
	ORDER BY 
		DateFrom asc, DateTo asc ;			

	DECLARE @recordsCount int = 0;
	SET @recordsCount = (select COUNT(RecordId) from #order_items)

	-- NO relevant "order items" found
	IF (@recordsCount = 0)
	BEGIN
		RETURN
	END

	------------------------------------------------------------------------------
	-- Try to get TotalMonthlySalary FROM CompanyEmployeeCount
	------------------------------------------------------------------------------

	DECLARE @salary numeric(18,2) = null

	SELECT
		TOP 1
		@salary = e.TotalMonthlySalary	
	FROM	
		dbo.MP_CustomerMarketPlace AS mp 
		INNER JOIN dbo.Customer AS cust ON (mp.CustomerId = cust.Id and mp.Id = @MpID) 
		INNER JOIN dbo.CompanyEmployeeCount AS e ON (e.CustomerId = cust.Id )		
	ORDER BY 
		e.Created DESC;

	------------------------------------------------------------------------------
	-- Try to get salaries FROM RtiTaxMonth data
	------------------------------------------------------------------------------

	create table #rti_salaries(	
		mpID int,
		recordID int,
		dateStart DATETIME,
		dateEnd DATETIME,
		amountPaid numeric (18,2)
	)

	IF @salary is null
	BEGIN
		insert into #rti_salaries (mpID, recordID, dateStart, dateEnd, amountPaid)	
		select					
			@MpID, 
			taxrec.CustomerMarketPlaceUpdatingHistoryRecordId ,
			taxen.DateStart,
			taxen.DateEnd,
			taxen.AmountPaid	
		FROM dbo.MP_RtiTaxMonthRecords as taxrec inner join dbo.MP_RtiTaxMonthEntries as taxen on (taxen.RecordId = taxrec.Id )
		where 		
			taxrec.CustomerMarketPlaceUpdatingHistoryRecordId in (select RecordId from #order_items)
		order by 
			DateStart asc, DateEnd asc 
	END

	------------------------------------------------------------------------------
	-- Storing temp results
	------------------------------------------------------------------------------

	create table #months
	(	recordID int,
		amount numeric (18,2),
		boxNum int,
		monthStart DATETIME,
		monthEnd DATETIME,
		dateRatio numeric(18,8),
		revenue numeric(18,2) DEFAULT 0.00,	
		opex numeric(18,2) DEFAULT 0.00,	
		salary numeric(18,2) DEFAULT 0.00,
		tax numeric(18,2) DEFAULT 0.00					-- TEMP field/value for future usage
		--,actualLoanPayment numeric(18,2) DEFAULT 0.00	-- TEMP field/value for future usage
	)
					
	DECLARE @recordID int = null
	DECLARE @dateFrom DATETIME
	DECLARE @dateTo DATETIME
	DECLARE @amount numeric(18,2)
	DECLARE @boxNum int = null

	DECLARE orderitems_cursor CURSOR FOR SELECT RecordID, DateFrom, DateTo, Amount, BoxNum FROM #order_items

	OPEN orderitems_cursor
	FETCH NEXT FROM orderitems_cursor INTO @recordID, @dateFrom, @dateTo, @amount, @boxNum

	WHILE @@FETCH_STATUS = 0   
		BEGIN   
    
			DECLARE @monthStart DATETIME = dbo.udfMonthStart(@dateFrom) 
			DECLARE @monthEnd DATETIME = null

			WHILE @monthStart <= @dateTo
				BEGIN

					SET @monthEnd = DATEADD(second, -1, DATEADD(month, 1, @monthStart))	;  -- monthEnd

					insert into #months 
					(	recordID ,					
						amount ,
						boxNum ,
						monthStart ,
						monthEnd ,
						dateRatio --,
					--	revenue ,
					--	opex ,
					--	salary
					)		
					values
					(	@recordID ,
						@amount ,
						@boxNum ,
						@monthStart,	--monthStart
						@monthEnd,		-- monthEnd
						dbo.udfDateIntersectionRatio(@monthStart, @monthEnd, @dateFrom, dbo.udfJustBeforeMidnight(@dateTo))  -- dateRatio
						--0 ,			-- Turnover (revenue)
						--0 ,			-- opex,
						--@salary		-- salary			
					);

					SET @monthStart = DATEADD(month, 1, @monthStart) ; -- next monthStart
				END

		FETCH NEXT FROM orderitems_cursor INTO @recordID, @dateFrom, @dateTo, @amount, @boxNum
	END   

	CLOSE orderitems_cursor   
	DEALLOCATE orderitems_cursor

	------------------------------------------------------------------------------
	-- UPDATE months temp data with money data distribution: revenue (box6), opex (box7), salary
	------------------------------------------------------------------------------

	UPDATE t1
	  SET 
		t1.revenue = ISNULL(t1.dateRatio*t1.amount, 0),
		t1.opex =  ISNULL(t1.dateRatio*t1.amount, 0),
		t1.salary = 
		( CASE WHEN (@salary IS NULL and t2.amountPaid IS NOT NULL) THEN t2.amountPaid		
			ELSE ISNULL(@salary, 0)
		 END )
	  FROM 
		#months AS t1 
		LEFT JOIN #rti_salaries as t2 on ( t1.recordID = t2.recordID and (MONTH(t2.dateStart) = MONTH(t1.monthStart) AND YEAR(t2.dateStart) = YEAR(t1.monthStart)) )	

	------------------------------------------------------------------------------
	-- Fill in the final #aggregatedmonths with Turnover  (revenue==box6)
	------------------------------------------------------------------------------	
	
	set @amount = 0.00

	select 
		monthStart as TheMonth,
		monthEnd as NextMonth,
		recordID as RecordID, 
		revenue as Turnover,		
		@amount as ValueAdded,
		@amount as FreeCashFlow
	into
		#aggregatedmonths
	 from 
		#months 	 
	 where boxNum = 6

	------------------------------------------------------------------------------
	-- Update #aggregatedmonths with calculations : ValueAdded = (Revenues - Opex), FreeCashFlow = (ValueAdded - Salaries - Tax - ActualLoanPayment)
	------------------------------------------------------------------------------	

	UPDATE #aggregatedmonths 
	SET
		ValueAdded = (t1.Turnover - t2.opex),
		FreeCashFlow = ISNULL(((t1.Turnover - t2.opex ) - t2.salary - t2.tax), 0)
	FROM
		#aggregatedmonths t1  inner join #months t2 on ((t1.TheMonth = t2.monthStart and t1.NextMonth = t2.monthEnd) and t2.boxNum = 7) 
	

	------------------------------------------------------------------------------
	-- At this point #aggregatedmonths contains new data.
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------
	 
	 UPDATE HmrcAggregation SET
		IsActive = 0
	FROM
		HmrcAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #aggregatedmonths m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO HmrcAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,		
		ValueAdded,
		FreeCashFlow
	)
	SELECT
		TheMonth,
		1, -- IsActive
		@HistoryID,
		Turnover,		
		ValueAdded,
		FreeCashFlow
	FROM
		#aggregatedmonths

	------------------------------------------------------------------------------

	COMMIT TRANSACTION
	
	
END
GO