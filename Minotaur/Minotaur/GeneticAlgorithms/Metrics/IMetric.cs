namespace Minotaur.GeneticAlgorithms.Metrics {
	using Minotaur.GeneticAlgorithms.Population;

	public interface IMetric {
		string Name { get; }
		float EvaluateAsMaximizationTask(Individual individual);
	}
}

