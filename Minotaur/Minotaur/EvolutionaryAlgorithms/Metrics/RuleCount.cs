namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class RuleCount: IMetric {

		public string Name => nameof(RuleCount);

		public float EvaluateAsMaximizationTask(Individual individual) => -1 * individual.Rules.Length;
	}
}
