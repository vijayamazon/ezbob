IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE name = 'TotalsMonthTail')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description, IsEncrypted) VALUES (
		'TotalsMonthTail', '3', 'Int. Number of days before end of the month which defines "month tail". So if month tail is 3 days then:
* for 31 days month tail is 29th, 30th, and 31th
* for 30 days month tail is 28th, 29th, and 30th.
* for regular February tail is 26th, 27th, and 28th.
* for leap February tail is 27th, 28th, 29th.

Usage: if current date and last marketplace check date are in "month tail" then "last n months" are calculated starting with current month, otherwise starting with the previous month.', 0
	)
END
GO
