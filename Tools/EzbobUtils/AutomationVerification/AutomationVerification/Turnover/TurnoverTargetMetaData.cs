namespace AutomationCalculator.Turnover {
	using System;
	using System.Collections.Generic;

	public class TurnoverTargetMetaData {
		public TurnoverTargetMetaData(
			SortedDictionary<int, AOneMonthValue> target,
			Func<AOneMonthValue> creator 
		) {
			Target = target;
			this.itemCreator = creator;
		} // constructor

		public SortedDictionary<int, AOneMonthValue> Target { get; private set; }

		public AOneMonthValue CreateItem(TurnoverDbRow r) {
			AOneMonthValue t = this.itemCreator();
			t.Add(r);
			return t;
		} // CreateItem

		public bool HasItemCreator {
			get { return this.itemCreator != null; }
		} // HasItemCreator

		private readonly Func<AOneMonthValue> itemCreator;
	} // class TurnoverTargetMetaData

	public static class TurnoverTargetMetaDataExt {
		public static bool IsValid(this TurnoverTargetMetaData meta) {
			return (meta != null) && (meta.Target != null) && meta.HasItemCreator;
		} // IsValid

		public static void Add(this TurnoverTargetMetaData meta, TurnoverDbRow row, params int[] monthCountList) {
			if (!meta.IsValid())
				return;

			foreach (int monthCount in monthCountList) {
				if (row.MonthCount > monthCount)
					continue;

				if (meta.Target.ContainsKey(monthCount))
					meta.Target[monthCount].Add(row);
				else
					meta.Target[monthCount] = meta.CreateItem(row);
			} // for
		} // Add
	} // class TurnoverTargetMetaDataExt
} // namespace
