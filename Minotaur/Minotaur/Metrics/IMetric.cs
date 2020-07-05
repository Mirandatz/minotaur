namespace Minotaur.Metrics {
	using Minotaur.Classification;

	public interface IMetric {
		string Name { get; }
		float EvaluateAsMaximizationTask(ConsistentModel model);
		float EvaluateToHumanReadable(ConsistentModel model);
	}
}

