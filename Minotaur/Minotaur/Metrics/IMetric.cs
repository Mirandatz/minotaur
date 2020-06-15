namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using Minotaur.EvolutionaryAlgorithms.Population;

	public interface IMetric {
		string Name { get; }
		float EvaluateAsMaximizationTask(Individual individual);
	}
}

