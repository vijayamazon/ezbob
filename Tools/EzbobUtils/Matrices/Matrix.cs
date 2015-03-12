namespace Ezbob.Matrices {
	using System;
	using System.Collections.Generic;
	using Ezbob.ValueIntervals;

	public class Matrix<T> where T: IComparable<T> {
		public Matrix() {
			this.rows = new List<TInterval<T>>();
		} // constructor

		private List<TInterval<T>> rows;
	} // class Matrix
} // namespace
