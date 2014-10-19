namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal interface IType {
		string Name { get; }
		string FullName { get; }
		bool CanBeNull { get; }
		Type UnderlyingType { get; }
		string Hint { get; set; }
		IType Clone();
		object CreateInstance(string sValue);
		bool Differs(IType oPrevious);
	} // interface IType
} // namespace
