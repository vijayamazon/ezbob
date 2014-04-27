namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class RunningBalance
	{
		public double Balance { get; set; }
		public DateTime Date { get; set; }
	}

	[Serializable]
	public class YodleeRunningBalanceModel
	{
		public SortedDictionary<string /*yearmonth*/, RunningBalance> LowRunningBalanceDict { get; set; }
		public SortedDictionary<string /*yearmonth*/, RunningBalance> HighRunningBalanceDict { get; set; }
		public SortedDictionary<string /*accountNum*/, double /*currentBalance*/> AccountCurrentBalanceDict { get; set; }
		public SortedDictionary<string /*date*/, double> MergedDailyRunningBalanceDict { get; set; }
		public double BankFrame = 0;
		public DateTime AsOfDate;
	}

	public class YodleeRunningBalanceModelBuilder
	{
		private SortedDictionary<DateTime, Dictionary<string /*AccountNum*/, double /*RunningBalance*/>> RunningBalanceDict { get; set; }
		private DateTime _firstTrans;
		private DateTime _lastTrans;

		private YodleeRunningBalanceModel yodlee;
		public YodleeRunningBalanceModelBuilder()
		{
			yodlee = new YodleeRunningBalanceModel
				{
					LowRunningBalanceDict = new SortedDictionary<string, RunningBalance>(),
					HighRunningBalanceDict = new SortedDictionary<string, RunningBalance>(),
					AsOfDate = DateTime.Today,
					AccountCurrentBalanceDict = new SortedDictionary<string, double>(),
				};

			RunningBalanceDict = new SortedDictionary<DateTime, Dictionary<string, double>>();
			_firstTrans = DateTime.Today;
			_lastTrans = DateTime.Today;
			
		}

		public void Add(YodleeTransactionModel transaction, string accountNum)
		{
			var date = transaction.transactionDate;
			var runningBalance = transaction.runningBalance.HasValue ? transaction.runningBalance.Value : 0;

			AddRunningBalance(runningBalance, date, accountNum);

			if (_firstTrans > date)
			{
				_firstTrans = date;
			}

			if (_lastTrans == DateTime.Today)
			{
				_lastTrans = yodlee.AsOfDate;
			}

			if (_lastTrans < date)
			{
				_lastTrans = date;
			}
		}

		private void AddRunningBalance(double runningBalance, DateTime date, string accountNum)
		{
			if (!RunningBalanceDict.ContainsKey(date.Date))
			{
				var rbd = new Dictionary<string, double>();
				rbd[accountNum] = runningBalance;
				RunningBalanceDict[date.Date] = rbd;
			}
			else
			{
				RunningBalanceDict[date.Date][accountNum] = runningBalance;
			}
		}

		public void CalculateMergedRunningBalace()
		{
			CalculateMissingDated();
			CalculateMergedData();
			CalculateMinMaxMonthlyRunningBalance();
		}

		private void CalculateMissingDated()
		{
			_lastTrans = yodlee.AsOfDate > _lastTrans ? yodlee.AsOfDate.Date : _lastTrans;
			if (!RunningBalanceDict.ContainsKey(_lastTrans.Date))
			{
				var rbd = new Dictionary<string, double>();
				foreach (var accCurBalance in yodlee.AccountCurrentBalanceDict)
				{
					rbd[accCurBalance.Key] = accCurBalance.Value;
				}
				RunningBalanceDict[_lastTrans.Date] = rbd;
			}
			_lastTrans = _lastTrans.AddDays(-1);
			while (_firstTrans <= _lastTrans)
			{
				if (!RunningBalanceDict.ContainsKey(_lastTrans.Date))
				{
					RunningBalanceDict[_lastTrans.Date] = RunningBalanceDict[_lastTrans.AddDays(1).Date];
				}
				else
				{
					var curr = RunningBalanceDict[_lastTrans.Date];
					var tommorow = RunningBalanceDict[_lastTrans.AddDays(1).Date];
					foreach (var runningBalanceTommorow in tommorow)
					{
						if (!curr.ContainsKey(runningBalanceTommorow.Key))
						{
							curr[runningBalanceTommorow.Key] = runningBalanceTommorow.Value;
						}
					}
				}
				_lastTrans = _lastTrans.AddDays(-1);
			}
		}

		private void CalculateMergedData()
		{
			yodlee.MergedDailyRunningBalanceDict = new SortedDictionary<string, double>();
			foreach (var rb in RunningBalanceDict)
			{
				foreach (var balance in rb.Value)
				{
					if (!yodlee.MergedDailyRunningBalanceDict.ContainsKey(rb.Key.Ticks.ToString()))
					{
						yodlee.MergedDailyRunningBalanceDict[rb.Key.Ticks.ToString()] = balance.Value;
					}
					else
					{
						yodlee.MergedDailyRunningBalanceDict[rb.Key.Ticks.ToString()] += balance.Value;
					}
				}
			}
		}

		private void CalculateMinMaxMonthlyRunningBalance()
		{
			foreach (var mdrbd in yodlee.MergedDailyRunningBalanceDict)
			{
				DateTime date =new DateTime(long.Parse(mdrbd.Key));
				string yearmonth = (date.Year * 100 + date.Month).ToString();
				if (!yodlee.LowRunningBalanceDict.ContainsKey(yearmonth))
				{
					yodlee.LowRunningBalanceDict[yearmonth] = new RunningBalance { Date = date, Balance = mdrbd.Value };
				}
				else
				{
					if (yodlee.LowRunningBalanceDict[yearmonth].Balance > mdrbd.Value)
					{
						yodlee.LowRunningBalanceDict[yearmonth].Balance = mdrbd.Value;
						yodlee.LowRunningBalanceDict[yearmonth].Date = date;
					}
				}

				if (!yodlee.HighRunningBalanceDict.ContainsKey(yearmonth))
				{
					yodlee.HighRunningBalanceDict[yearmonth] = new RunningBalance { Date = date, Balance = mdrbd.Value };
				}
				else
				{
					if (yodlee.HighRunningBalanceDict[yearmonth].Balance < mdrbd.Value)
					{
						yodlee.HighRunningBalanceDict[yearmonth].Balance = mdrbd.Value;
						yodlee.HighRunningBalanceDict[yearmonth].Date = date;
					}
				}
			}
		}

		public void SetBankFrame(double overdraftProtection)
		{
			yodlee.BankFrame -= overdraftProtection;
		}

		public void SetAsOfDate(DateTime asOfDate)
		{
			yodlee.AsOfDate = asOfDate;
		}

		public void SetAccountCurrentBalance(string accountNumber, double balance)
		{
			yodlee.AccountCurrentBalanceDict[accountNumber] = balance;
		}

		public YodleeRunningBalanceModel GetModel()
		{
			return yodlee;
		}
	}
}