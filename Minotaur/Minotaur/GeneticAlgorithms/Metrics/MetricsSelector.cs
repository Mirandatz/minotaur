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
				switch (metricsNames[i]) {

				case "fscore":
				metrics[i] = new FScore(dataset);
				break;

				case "model-size":
				metrics[i] = new ModelSize();
				break;

				case "average-rule-volume":
				metrics[i] = new AverageRuleVolume(dataset);
				break;

				default:
				throw new ArgumentException($"Unsupported metric: {currentMetricName}");
				}
			}

			return metrics;
		}
	}
}
