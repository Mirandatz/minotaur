namespace Minotaur.Metrics {
	using System;
	using Minotaur.Classification;

	public sealed class RuleCount: IMetric {

		public string Name { get; } = "Rule Count";

		public float EvaluateAsMaximizationTask(ConsistentModel model) => (-1) * (model.Rules.Count);

		public float EvaluateToHumanReadable(ConsistentModel model) => model.Rules.Count;

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
