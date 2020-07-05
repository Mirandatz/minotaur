namespace Minotaur.Metrics {
	using Minotaur.Classification;

	public sealed class RuleCount: IMetric {

		public string Name { get; } = "Rule Count";

		public float EvaluateAsMaximizationTask(ConsistentModel model) => (-1) * (model.Rules.Count);

		public float EvaluateToHumanReadable(ConsistentModel model) => model.Rules.Count;
	}
}
