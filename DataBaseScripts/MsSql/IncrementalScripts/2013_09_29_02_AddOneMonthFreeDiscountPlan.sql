IF NOT EXISTS (SELECT 1 FROM DiscountPlan WHERE Name = 'One month free')
BEGIN
	INSERT INTO DiscountPlan VALUES ('One month free', '0,0,0,-99.9,0,0,0,0,0,0,0,0', 0)
END
GO
