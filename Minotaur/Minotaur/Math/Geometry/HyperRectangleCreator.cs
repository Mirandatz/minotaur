namespace Minotaur.Math.Geometry {
	using Minotaur.Classification.Rules;

	public static class HyperRectangleCreator {

		public static HyperRectangle FromRuleAntecedent(Antecedent ruleAntecedent) {

			var featureTests = ruleAntecedent.AsSpan();
			var intervals = new Interval[featureTests.Length];

			for (int i = 0; i < featureTests.Length; i++)
				intervals[i] = featureTests[i].Interval;

			return new HyperRectangle(intervals);
		}
	}
}
