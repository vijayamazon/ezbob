
namespace TestRailConsoleClient.Tests {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
	public class TestCaseIDAttribute : Attribute {
		private readonly uint caseID;

		public TestCaseIDAttribute(uint caseID) {
			this.caseID = caseID;
		}

		public uint CaseID { get { return this.caseID; } }
	}
}
