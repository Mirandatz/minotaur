namespace Minotaur.Theseus {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;
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

		public HyperRectangle[] FromRules(Array<Rule> rules) {
			var boxes = new HyperRectangle[rules.Length];

			for (int i = 0; i < boxes.Length; i++) {
				var rule = rules[i];
				var box = FromRule(rule);
				boxes[i] = box;
			}

			return boxes;
		}

		public HyperRectangle FromRule(Rule rule) {
			var antecedent = rule.Antecedent;
			return FromRuleAntecedent(antecedent);
		}

		public HyperRectangle FromRuleAntecedent(Array<IFeatureTest> ruleAntecedent) {
			var intervals = new IInterval[ruleAntecedent.Length];

			for (int i = 0; i < intervals.Length; i++) {
				var test = ruleAntecedent[i];
				var interval = _testIntervalConverter.FromFeatureTest(test);
				intervals[i] = interval;
			}

			return new HyperRectangle(intervals);
		}
	}
}
