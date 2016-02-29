namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;

	internal abstract class AEnum<TMember, IntType> : ARepository<TMember>
		where IntType : struct
		where TMember : EnumMember<IntType>, new()
	{
		public virtual TMember Find(IntType id) {
			foreach (var m in this) {
				if (m == null)
					continue;

				if (m.Value.Equals(id))
					return m;
			} // for each

			return null;
		} // Find

		public virtual TMember Find(string commCode) {
			return this.Where(m => m != null).FirstOrDefault(m => m.SearchKey.Equals(commCode));
		} // Find

		protected AEnum(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor
	} // class ARepository
} // namespace
