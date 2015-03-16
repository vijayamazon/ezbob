IF OBJECT_ID('BrokerSetupFeeMap') IS NULL
BEGIN
CREATE TABLE BrokerSetupFeeMap(
	 Id INT NOT NULL IDENTITY(1,1)
	,MinAmount INT NOT NULL
	,MaxAmount INT NOT NULL
	,Fee INT NOT NULL
	,CONSTRAINT PK_BrokerSetupFeeMap PRIMARY KEY (Id), 
)

INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (1000, 9999, 500)
INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (10000, 19999, 1250)
INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (20000, 29999, 1800)
INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (30000, 39999, 2300)
INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (40000, 49999, 2600)
INSERT INTO BrokerSetupFeeMap(MinAmount, MaxAmount, Fee) VALUES (50000, 59999, 3000)
END 