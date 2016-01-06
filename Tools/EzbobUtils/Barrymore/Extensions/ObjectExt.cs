namespace Ezbob.Utils.Extensions {
	using System.Collections;
	using System.Text;

	public static class ObjectExt {
		public static string ToLogStr(this object target, int level = 0) {
			// If by any chance operator '==' was overridden for the type then not-null reference
			// can point to a "null" object.
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (ReferenceEquals(target, null) || (target == null))
				return "-- null --";

			if (TypeUtils.IsPlainType(target.GetType()))
				return target.ToString();

			if (level > 10)
				return string.Format("Not expanding type {0}: nesting level exceeded.", target.GetType());

			var builder = new StringBuilder();

			if (TypeUtils.IsEnumerable(target.GetType())) {
				int counter = 1;

				foreach (var item in (IEnumerable)target) {
					builder.AppendFormat("Item #{0}: {1}\n", counter, item.ToLogStr(level + 1));
					counter++;
				} // for each

				return string.Format("Enumerable with {0} items.\n{1}-- End of enumeration.\n", counter - 1, builder);
			} // if

			bool appended = false;

			target.TraverseReadable((ignored, propInfo) => {
				appended = true;

				builder.AppendFormat(
					"{0} = '{1}' (of type {2})\n",
					propInfo.Name,
					propInfo.GetValue(target).ToLogStr(level + 1),
					propInfo.PropertyType
				);
			});

			if (!appended)
				builder.AppendFormat("{0}", target);

			return builder.ToString();
		} // ToLogStr
	} // class ObjectExt
} // namespace
