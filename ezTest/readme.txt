Небольшой FAQ:
1. Для выполнения одного теста в цикле

FOR /L %i IN (1,1,100) DO "C:\_WORKPLACE\casperjs\batchbin\casperjs.bat" "testcase2 Wizard pass.coffee"
где
	FOR /L %i IN (1 - начальное значение, 1 - шаг, 100 - конечное значение)
	"C:\_WORKPLACE\casperjs\batchbin\casperjs.bat" - расположение файла casperjs
	"testcase2 Wizard pass.coffee" - сам тест