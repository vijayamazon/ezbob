IF NOT EXISTS (SELECT * FROM MP_MarketPlaceType WHERE InternalId = 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0')
BEGIN
	INSERT INTO MP_MarketPlaceType VALUES ('PayPoint', 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0', 'PayPoint', 1)
END
GO
