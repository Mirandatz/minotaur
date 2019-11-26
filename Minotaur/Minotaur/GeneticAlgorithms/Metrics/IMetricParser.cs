namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public static class IMetricParser {

		public static IMetric[] ParseMetrics(
			Dataset dataset,
			Array<string> metricsNames,
			ClassificationType classificationType
			) {
			if (metricsNames.IsEmpty)
				throw new ArgumentException(nameof(metricsNames) + " can't be empty.");

			var metrics = new IMetric[metricsNames.Length];

			for (int i = 0; i < metricsNames.Length; i++) {

				var currentMetricName = metricsNames[i];
				metrics[i] = (metricsNames[i], classificationType) switch
				{
					("fscore", ClassificationType.SingleLabel) => new SingleLabelFScore(dataset),
					("fscore", ClassificationType.MultiLabel) => new MultiLabelFScore(dataset),
					("average-rule-volume", _) => new AverageRuleVolume(dataset),
					("rule-count", _) => new RuleCount(),
					("model-size", _) => new ModelSize(),

					_ => throw new ArgumentException($"Unsupported (Metric, ClassificationType): ({currentMetricName}, {classificationType})."),
				};
			}

			return metrics;
		}
	}
}
