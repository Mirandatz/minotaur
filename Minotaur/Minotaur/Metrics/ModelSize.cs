namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using System;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class ModelSize: IMetric {

		public string Name => nameof(ModelSize);

		public float Evaluate(Individual individual) => throw new NotImplementedException();

		public float EvaluateAsMaximizationTask(Individual individual) => -1 * Evaluate(individual);
	}
}
