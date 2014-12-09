using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ezbob.ValueIntervals {

	public interface IOrdinal<T> {
		/// <summary>
		/// Create new instance: decrease current value by one.
		/// I.e. if value is DateTime.Now this method should .AddDays(-1)
		/// if value is 5 this methos should set value to 4.
		/// </summary>
		T Previous();

		/// <summary>
		/// Create new instance: increase current value by one.
		/// I.e. if value is DateTime.Now this method should .AddDays(1)
		/// if value is 5 this methos should set value to 6.
		/// </summary>
		T Next();
	} // interface IOrdinal

} // namespace Ezbob.ValueIntervals
