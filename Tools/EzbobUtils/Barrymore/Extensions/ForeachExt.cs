﻿namespace Ezbob.Utils.Extensions {
	using System.Collections.Generic;

	public static class ForeachExt {

		public sealed class Item<T> {
			public int Index { get; set; }
			public T Value { get; set; }
			public bool IsLast { get; set; }
		}

		public static IEnumerable<Item<T>> WithIndex<T>(IEnumerable<T> enumerable) {
			Item<T> item = null;
			foreach (T value in enumerable) {
				Item<T> next = new Item<T>();
				next.Index = 0;
				next.Value = value;
				next.IsLast = false;
				if (item != null) {
					next.Index = item.Index + 1;
					yield return item;
				}
				item = next;
			}
			if (item != null) {
				item.IsLast = true;
				yield return item;
			}
		}

	}
}
