
IF NOT EXISTS (SELECT * FROM YodleeBanks WHERE ContentServiceId=22676)
BEGIN 
INSERT INTO YodleeBanks(Name, ContentServiceId, ParentBank, Active, [Image]) VALUES	('TSB (UK)', 22676, '', 1, 0)
END 
GO

IF NOT EXISTS (SELECT * FROM YodleeBanks WHERE ContentServiceId=22676)
BEGIN 
INSERT INTO YodleeBanks(Name, ContentServiceId, ParentBank, Active, [Image]) VALUES ('TSB Business Banking (UK)', 22677, '', 1, 0)
END 
GO