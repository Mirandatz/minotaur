namespace Minotaur.Theseus {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class RuleAntecedentHyperRectangleConverter {
		public readonly Dataset Dataset;
		private readonly FeatureTestDimensionIntervalConverter _testIntervalConverter;

		public RuleAntecedentHyperRectangleConverter(FeatureTestDimensionIntervalConverter testIntervalConverter) {
			_testIntervalConverter = testIntervalConverter;
			Dataset = _testIntervalConverter.Dataset;
		}

		public IFeatureTest[] FromHyperRectangle(HyperRectangle hyperRectangle) {
			var tests = new IFeatureTest[hyperRectangle.DimensionCount];

			for (int i = 0; i < tests.Length; i++) {
				var interval = hyperRectangle.GetDimensionInterval(i);
				var test = _testIntervalConverter.FromDimensionInterval(interval);
				tests[i] = test;
			}

			return tests;
		}

		public HyperRectangle FromRuleAntecedent(Array<IFeatureTest> ruleAntecedent) {
			var intervals = new IDimensionInterval[ruleAntecedent.Length];

			for (int i = 0; i < intervals.Length; i++) {
				var test = ruleAntecedent[i];
				var interval = _testIntervalConverter.FromFeatureTest(test);
				intervals[i] = interval;
			}

			return new HyperRectangle(intervals);
		}
	}
}
