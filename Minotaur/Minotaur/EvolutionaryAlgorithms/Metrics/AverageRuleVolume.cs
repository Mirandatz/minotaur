namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class AverageRuleVolume: IMetric {

		private readonly Dataset _dataset;

		public AverageRuleVolume(Dataset dataset) {
			_dataset = dataset;
		}

		public string Name => nameof(AverageRuleVolume);

		public float EvaluateAsMaximizationTask(Individual individual) => throw new NotImplementedException();
	}
}