namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using NHibernate;

	[Serializable]
	public class RunningBalance
	{
		public double Balance { get; set; }
		public DateTime Date { get; set; }
	}

	public class YodleeRunningBalanceModel
	{
		public SortedDictionary<int/*yearmonth*/, RunningBalance> LowRunningBalanceDict { get; set; }
		public SortedDictionary<int/*yearmonth*/, RunningBalance> HighRunningBalanceDict { get; set; }
		private SortedDictionary<DateTime, Dictionary<string /*AccountNum*/, double /*RunningBalance*/>> RunningBalanceDict { get; set; }
		public SortedDictionary<DateTime, double> MergedDailyRunningBalanceDict { get; set; }
		public double BankFrame = 0;
		public DateTime AsOfDate;
		private DateTime _firstTrans;
		private DateTime _lastTrans;
		public SortedDictionary<string/*accountNum*/, double/*currentBalance*/> AccountCurrentBalanceDict { get; set; }
		public YodleeRunningBalanceModel()
		{
			LowRunningBalanceDict = new SortedDictionary<int, RunningBalance>();
			HighRunningBalanceDict = new SortedDictionary<int, RunningBalance>();
			RunningBalanceDict = new SortedDictionary<DateTime, Dictionary<string, double>>();
			AccountCurrentBalanceDict = new SortedDictionary<string, double>();
			_firstTrans = DateTime.Today;
			_lastTrans = DateTime.Today;
			AsOfDate = DateTime.Today;
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
				_lastTrans = AsOfDate;
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
			_lastTrans = AsOfDate > _lastTrans ? AsOfDate.Date : _lastTrans;
			if (!RunningBalanceDict.ContainsKey(_lastTrans.Date))
			{
				var rbd = new Dictionary<string, double>();
				foreach (var accCurBalance in AccountCurrentBalanceDict)
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
			MergedDailyRunningBalanceDict = new SortedDictionary<DateTime, double>();
			foreach (var rb in RunningBalanceDict)
			{
				foreach (var balance in rb.Value)
				{
					if (!MergedDailyRunningBalanceDict.ContainsKey(rb.Key))
					{
						MergedDailyRunningBalanceDict[rb.Key] = balance.Value;
					}
					else
					{
						MergedDailyRunningBalanceDict[rb.Key] += balance.Value;
					}
				}
			}
		}

		private void CalculateMinMaxMonthlyRunningBalance()
		{
			foreach (var mdrbd in MergedDailyRunningBalanceDict)
			{
				var yearmonth = mdrbd.Key.Year * 100 + mdrbd.Key.Month;
				if (!LowRunningBalanceDict.ContainsKey(yearmonth))
				{
					LowRunningBalanceDict[yearmonth] = new RunningBalance { Date = mdrbd.Key, Balance = mdrbd.Value };
				}
				else
				{
					if (LowRunningBalanceDict[yearmonth].Balance > mdrbd.Value)
					{
						LowRunningBalanceDict[yearmonth].Balance = mdrbd.Value;
						LowRunningBalanceDict[yearmonth].Date = mdrbd.Key;
					}
				}

				if (!HighRunningBalanceDict.ContainsKey(yearmonth))
				{
					HighRunningBalanceDict[yearmonth] = new RunningBalance { Date = mdrbd.Key, Balance = mdrbd.Value };
				}
				else
				{
					if (HighRunningBalanceDict[yearmonth].Balance < mdrbd.Value)
					{
						HighRunningBalanceDict[yearmonth].Balance = mdrbd.Value;
						HighRunningBalanceDict[yearmonth].Date = mdrbd.Key;
					}
				}
			}
		}
	}
}