namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	public class YodleeAccountItem : TimeDependentRangedDataBase {
		public BankData _Data { get; private set; }

		public override DateTime RecordTime {
			get {
				return (_Data.asOfDate != null && _Data.asOfDate.date.HasValue) ? _Data.asOfDate.date.Value : DateTime.Today;
			}
		}

		public YodleeAccountItem(BankData data) {
			_Data = data;
		}
	} // class YodleeAccountItem

	public class YodleeAccountList : ReceivedDataListTimeMarketTimeDependentBase<YodleeAccountItem> {
		public YodleeAccountList(DateTime submittedDate, IEnumerable<YodleeAccountItem> collection = null)
			: base(submittedDate, collection) { }

		public static YodleeAccountList Create(DateTime submittedDate, YodleeOrderDictionary dictionary) {
			List<YodleeAccountItem> list = dictionary.Data.Keys.Select(yodleeAccountItem => new YodleeAccountItem(yodleeAccountItem))
				.ToList();
			return new YodleeAccountList(submittedDate, list);
		}

		public override ReceivedDataListTimeDependentBase<YodleeAccountItem> Create(DateTime submittedDate, IEnumerable<YodleeAccountItem> collection) {
			return new YodleeAccountList(submittedDate, collection);
		}
	} // class YodleeAccountList

	[Serializable]
	public class YodleeOrderDictionary {
		public Dictionary<BankData, List<BankTransactionData>> Data { get; set; }
	} // class YodleeOrderDictionary

	public class YodleeTransactionItem : TimeDependentRangedDataBase {
		public BankTransactionData _Data { get; private set; }

		public override DateTime RecordTime {
			get {
				DateTime date;
				if (_Data.postDate != null && _Data.postDate.date.HasValue)
					date = _Data.postDate.date.Value;
				else if (_Data.transactionDate != null && _Data.transactionDate.date.HasValue)
					date = _Data.transactionDate.date.Value;
				else
					date = DateTime.Today;
				return date;
			}
		}

		public YodleeTransactionItem(BankTransactionData data) {
			_Data = data;
		}
	} // class YodleeTransactionItem

	public class YodleeTransactionList : ReceivedDataListTimeMarketTimeDependentBase<YodleeTransactionItem> {
		public YodleeTransactionList(DateTime submittedDate, IEnumerable<YodleeTransactionItem> collection = null)
			: base(submittedDate, collection) { }

		public static YodleeTransactionList Create(DateTime submittedDate, YodleeOrderDictionary dictionary) {
			List<YodleeTransactionItem> list = (from item in dictionary.Data.Keys from bankTransaction in dictionary.Data[item] select new YodleeTransactionItem(bankTransaction)).ToList();
			return new YodleeTransactionList(submittedDate, list);
		}

		public override ReceivedDataListTimeDependentBase<YodleeTransactionItem> Create(DateTime submittedDate, IEnumerable<YodleeTransactionItem> collection) {
			return new YodleeTransactionList(submittedDate, collection);
		}
	} // class YodleeTransactionList
} // namespace
