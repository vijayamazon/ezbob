IF OBJECT_ID('DefaultRateCompany') IS NULL
BEGIN
	CREATE TABLE DefaultRateCompany
	(
		Id INT IDENTITY,
		Start INT NOT NULL,
		[End] INT NOT NULL,
		Value DECIMAL (18, 6) NOT NULL
	)
	
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (0, 10, 0.5)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (11, 20, 0.101)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (21, 30, 0.07)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (31, 40, 0.058)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (41, 50, 0.051)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (51, 60, 0.042)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (61, 70, 0.033)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (71, 80, 0.026)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (81, 90, 0.019)
	INSERT INTO DefaultRateCompany (Start, [End], Value) VALUES (91, 10000000, 0.009)
END
GO

IF OBJECT_ID('DefaultRateCustomer') IS NULL
BEGIN
	CREATE TABLE DefaultRateCustomer
	(
		Id INT IDENTITY,
		Start INT NOT NULL,
		[End] INT NOT NULL,
		Value DECIMAL (18, 6) NOT NULL
	)
	
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (0, 479, 0.95)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (480, 559, 0.667)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (560, 639, 0.503)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (640, 719, 0.333)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (720, 799, 0.2)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (800, 879, 0.111)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (880, 959, 0.059)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (960, 1039, 0.03)
	INSERT INTO DefaultRateCustomer (Start, [End], Value) VALUES (1040, 10000000, 0.015)
END
GO
