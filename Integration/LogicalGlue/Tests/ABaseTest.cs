namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using NUnit.Framework;

	abstract class ABaseTest {
		[SetUp]
		public void Init() {
			var oLog4NetCfg = new Log4Net().Init();

			var env = oLog4NetCfg.Environment;
			Log = new ConsoleLog(new SafeILog(this));
			DB = new SqlConnection(oLog4NetCfg.Environment, Log);

			Ezbob.Integration.LogicalGlue.Library.Initialize(env, this.DB, Log);

			ConfigManager.CurrentValues.Init(DB, Log);
		} // Init

		protected ModelOutput CreateInternalModelOutput() {
			var mo = new ModelOutput {
				Status = "Status of the model",
				Grade = new Grade {
					Score = 28,
					EncodedResult = -1,
					DecodedResult = "Goooooooooooood",
					OutputRatios = new Dictionary<string, decimal>(),
				},
				Error = new ModelError {
					Warnings = new List<Warning> {
						new Warning { FeatureName = "feature", MaxValue = "100", MinValue = "0", Value = "ab", },
						new Warning { FeatureName = null, MaxValue = null, MinValue = null, Value = null, },
						new Warning { FeatureName = "FEATURE", MaxValue = "900", MinValue = "1", Value = "-1", },
					},
					ErrorCode = "error code",
					Exception = "some exception",
					Uuid = uuid,
					EncodingFailures = new List<EncodingFailure> {
						new EncodingFailure {
							ColumnName = "Bad encoded col",
							Message = "it's bad",
							Reason = "good reason",
							RowIndex = 0,
							UnencodedValue = "a value",
						},
						new EncodingFailure {
							ColumnName = null,
							Message = null,
							Reason = null,
							RowIndex = 0,
							UnencodedValue = null,
						},
						new EncodingFailure {
							ColumnName = "Another bad encoded col",
							Message = "it's really bad",
							Reason = "bad reason",
							RowIndex = 1,
							UnencodedValue = "another value",
						},
					},
					MissingColumns = new List<string> { "missing 0", "", "missing 1", null, "  ", "missing 2", },
				},
			};

			mo.Grade.OutputRatios[mo.Grade.DecodedResult] = 0.75m;
			mo.Grade.OutputRatios["Bad"] = 0.25m;

			return mo;
		} // CreateInternalModelOutput

		protected AConnection DB { get; private set; }
		protected ASafeLog Log { get; private set; }

		private static readonly Guid uuid = Guid.NewGuid();
	} // class ABaseTest
} // namespace
