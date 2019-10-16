namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	/// <summary>
	/// Wrote to be compatible with MULAN:
	/// https://github.com/tsoumakas/mulan/blob/f56e8148812dde357178a053cb09877515b538ad/mulan/src/main/java/mulan/evaluation/measure/InformationRetrievalMeasures.java
	/// </summary>
	public sealed class FScore: IMetric {
		private readonly Dataset _dataset;

		public string Name => nameof(FScore);

		public FScore(Dataset dataset) {
			_dataset = dataset;
		}

		public float Evaluate(Individual individual) {
			throw new NotImplementedException();
			//var predictions = individual.Predict(_dataset);
			//var confusionMatrix = PseudoConfusionMatrix.Create(
			//	actual: _dataset.InstanceLabels,
			//	predicted: predictions);

			//var tp = confusionMatrix.TruePositive;
			//var fp = confusionMatrix.FalsePositive;
			//var fn = confusionMatrix.FalseNegative;

			//if (tp + fp + fn == 0) {
			//	return 1;
			//}

			//var beta = 1;
			//var beta2 = beta * beta;
			//return ((beta2 + 1) * tp) / ((beta2 + 1) * tp + beta2 * fn + fp);
		}

		public float EvaluateAsMaximizationTask(Individual individual) => Evaluate(individual);
	}
}
