namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Collections.Generic;
	using System.Linq;

	internal static class Utility {
		internal static List<T> SetList<T>(List<T> target, IEnumerable<T> source) where T : class, ICanBeEmpty<T> {
			if (ReferenceEquals(source, target))
				return target;

			if (target == null)
				target = new List<T>();

			target.Clear();

			if (source == null)
				return target;

			target.AddRange(source.Where(w => (w != null) && !w.IsEmpty).Select(w => w.CloneTo()));

			return target;
		} // SetList
	} // class Utility
} // namespace
