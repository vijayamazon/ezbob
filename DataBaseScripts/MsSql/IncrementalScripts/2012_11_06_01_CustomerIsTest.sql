ALTER TABLE dbo.Customer ADD
	IsTest bit NULL
GO

UPDATE [dbo].[Customer]
   SET [IsTest] = 1
 WHERE 
	Name like '%gmail.comm' or 
	Name like '%q.q' or 
	Name like '%w.w' or 
	Name like '%qq.qq' or 
	Name like '%dal.da' or
	Name like '%dal.da' or
	Name like '%ww.ww' or
	Name like 'demo__@gmail.com' or
	Name like 'demo_@gmail.com' or
	Name like '%rtrtr.com'
GO