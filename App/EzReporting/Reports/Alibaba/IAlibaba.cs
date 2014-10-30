namespace Reports.Alibaba {
	using OfficeOpenXml;

	public interface IAlibaba {
		void Generate();
		ExcelPackage Report { get; }
	} // interface IAlibaba
} // namespace Reports.Alibaba
