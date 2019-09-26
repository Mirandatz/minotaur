namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public static class MetricsSelector {

		public static IMetric[] SelectMetrics(
			Dataset dataset,
			Array<string> metricsNames
			) {
			if (dataset is null)
				throw new ArgumentNullException(nameof(dataset));
			if (metricsNames is null)
				throw new ArgumentNullException(nameof(metricsNames));
			if (metricsNames.IsEmpty)
				throw new ArgumentException(nameof(metricsNames) + " can't be empty.");

			var metrics = new IMetric[metricsNames.Length];

			for (int i = 0; i < metricsNames.Length; i++) {

				var currentMetricName = metricsNames[i];

				metrics[i] = (metricsNames[i]) switch
				{
					"fscore" => new FScore(dataset),
					"model-size" => new ModelSize(),
					"average-rule-volume" => new AverageRuleVolume(dataset),
					"rule-count" => new RuleCount(),

					_ => throw new ArgumentException($"Unsupported metric: {currentMetricName}"),
				};
			}

			return metrics;
		}
	}
}
