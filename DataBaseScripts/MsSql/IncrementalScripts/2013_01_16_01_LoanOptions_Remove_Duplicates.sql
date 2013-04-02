/*удаление дубликатов из таблицы. ублируются записи в столбце LoanId*/
delete t1 FROM LoanOptions AS t1, LoanOptions AS t2
WHERE (t1.LoanId=t2.LoanId AND t1.Id>t2.Id)