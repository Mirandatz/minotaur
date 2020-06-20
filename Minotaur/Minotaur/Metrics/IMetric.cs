namespace Minotaur.Metrics {
	using Minotaur.Classification;

	public interface IMetric {
		string Name { get; }
		float EvaluateAsMaximizationTask(Model individual);
	}
}

