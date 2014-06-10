using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using NUnit.Framework;

	[TestFixture]
	class AutomationTestFixture : BaseTestFixture
	{
		[Test]
		public void test_mainstrat()
		{
			var ms = new MainStrategy(21370, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, _db, _log);
			ms.Execute();
		}
		
	}

}
