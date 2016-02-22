namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	public interface ICanBeEmpty<out T> {
		bool IsEmpty { get; }
		T CloneTo();
	} // interface ICanBeEmpty
} // namespace
