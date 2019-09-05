namespace Minotaur.GeneticAlgorithms.Metrics {
	using Minotaur.GeneticAlgorithms.Population;

	public interface IMetric {
		string Name { get; }
		float Evaluate(Individual individual);
		float EvaluateAsMaximizationTask(Individual individual);
	}
}

