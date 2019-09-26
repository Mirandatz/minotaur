namespace Minotaur.GeneticAlgorithms.Metrics {
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleCount: IMetric {

		public string Name => nameof(RuleCount);

		public float EvaluateAsMaximizationTask(Individual individual) => -1 * individual.Rules.Length;
	}
}
