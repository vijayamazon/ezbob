namespace Ezbob.Database {
	public interface IResultRow {
		/// <summary>
		/// Gets indicator whether current row is the first in a row set
		/// (for queries returning multiple row sets).
		/// </summary>
		/// <returns>"First in row set" indicator.</returns>
		bool IsFirst();

		/// <summary>
		/// Sets "first in row set" indicator.
		/// </summary>
		/// <param name="bIsFirst">"Is first" indicator.</param>
		void SetIsFirst(bool bIsFirst);
	} // interface IResultRow
} // namespace Ezbob.Database
