namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class SingleLabelFScore: IMetric {
		public string Name => throw new NotImplementedException();

		public SingleLabelFScore(Dataset dataset) {
			throw new NotImplementedException();
		}

		public float EvaluateAsMaximizationTask(Individual individual) {
			throw new NotImplementedException();
		}
	}
}
