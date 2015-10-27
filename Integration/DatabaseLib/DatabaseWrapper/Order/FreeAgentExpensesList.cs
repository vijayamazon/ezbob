namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	[Serializable]
	public class FreeAgentExpensesList : ReceivedDataListTimeMarketTimeDependentBase<FreeAgentExpense> {
		public FreeAgentExpensesList() : base(DateTime.Now, null) {}

		public FreeAgentExpensesList(DateTime submittedDate, IEnumerable<FreeAgentExpense> collection = null)
			: base(submittedDate, collection) {}

		public override ReceivedDataListTimeDependentBase<FreeAgentExpense> Create(
			DateTime submittedDate,
			IEnumerable<FreeAgentExpense> collection
		) {
			return new FreeAgentExpensesList(submittedDate, collection);
		} // Create
	} // class FreeAgentExpensesList

	[Serializable]
	public class FreeAgentExpenseAttachment {
		public string url { get; set; }
		public string content_src { get; set; }
		public string content_type { get; set; }
		public string file_name { get; set; }
		public int file_size { get; set; }
		public string description { get; set; }
	} // class FreeAgentExpenseAttachment

	[Serializable]
	public class FreeAgentExpenseCategory {
		public int Id { get; set; }
		public string url { get; set; }
		public string category_group { get; set; }
		public string description { get; set; }
		public string nominal_code { get; set; }
		public bool? allowable_for_tax { get; set; }
		public string tax_reporting_name { get; set; }
		public string auto_sales_tax_rate { get; set; }
	} // class FreeAgentExpenseCategory

	[Serializable]
	public class FreeAgentExpense : TimeDependentRangedDataBase {
		public string url { get; set; }
		public string user { get; set; }
		public string category { get; set; }
		public DateTime? dated_on { get; set; }
		public string currency { get; set; }
		public decimal gross_value { get; set; }
		public decimal native_gross_value { get; set; }
		public decimal? sales_tax_rate { get; set; }
		public decimal sales_tax_value { get; set; }
		public decimal native_sales_tax_value { get; set; }
		public string description { get; set; }
		public decimal? manual_sales_tax_amount { get; set; }
		public DateTime? updated_at { get; set; }
		public DateTime? created_at { get; set; }

		public FreeAgentExpenseAttachment attachment { get; set; }
		public FreeAgentExpenseCategory categoryItem { get; set; }

		public override DateTime RecordTime {
			get { return dated_on.HasValue ? dated_on.Value : new DateTime(1900, 1, 1); }
		} // RecordTime
	} // class FreeAgentExpense

	[Serializable]
	public class ExpensesListHelper : IFreeAgentItemContainer {
		public List<FreeAgentExpense> Expenses { get; set; }

		public bool HasItems() {
			return (Expenses != null) && (Expenses.Count > 0);
		} // HasItems

		public int GetItemCount() {
			return HasItems() ? Expenses.Count : 0;
		} // GetItemCount
	} // class ExpensesListHelper

	[Serializable]
	public class ExpenseCategoriesListHelper : IFreeAgentItemContainer {
		public List<FreeAgentExpenseCategory> admin_expenses_categories { get; set; }
		public List<FreeAgentExpenseCategory> cost_of_sales_categories { get; set; }
		public List<FreeAgentExpenseCategory> income_categories { get; set; }
		public List<FreeAgentExpenseCategory> general_categories { get; set; }

		public void UpdateSearchTree() {
			if (this.categories == null)
				this.categories = new SortedDictionary<string, FreeAgentExpenseCategory>();
			else
				this.categories.Clear();

			ListToTree(admin_expenses_categories);
			ListToTree(cost_of_sales_categories);
			ListToTree(income_categories);
			ListToTree(general_categories);
		} // UpdateSearchTree

		public FreeAgentExpenseCategory this[string url] {
			get {
				return this.categories.ContainsKey(url)
					? this.categories[url]
					: new FreeAgentExpenseCategory { url = string.Empty, description = "NOT FOUND " + url, };
			} // get
		} // indexer

		public bool HasItems() {
			return GetItemCount() > 0;
		} // HasItems

		public int GetItemCount() {
			int count = 0;

			if (admin_expenses_categories != null)
				count += admin_expenses_categories.Count;

			if (cost_of_sales_categories != null)
				count += cost_of_sales_categories.Count;

			if (income_categories != null)
				count += income_categories.Count;

			if (general_categories != null)
				count += general_categories.Count;

			return count;
		} // GetItemCount

		private void ListToTree(List<FreeAgentExpenseCategory> lst) {
			if (lst == null)
				return;

			foreach (FreeAgentExpenseCategory cat in lst)
				this.categories[cat.url] = cat;
		} // ListToTree

		private SortedDictionary<string, FreeAgentExpenseCategory> categories;
	} // class ExpenseCategoriesListHelper
} // namespace
