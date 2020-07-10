namespace Minotaur.Metrics {
	using System;
	using Minotaur.Datasets;

	public static class IMetricParser {

		public static IMetric[] ParseMetrics(Dataset dataset, ReadOnlySpan<string> metricsNames) {
			if (metricsNames.Length == 0)
				throw new ArgumentException(nameof(metricsNames) + " can't be empty.");

			var metrics = new IMetric[metricsNames.Length];

			for (int i = 0; i < metricsNames.Length; i++) {
				metrics[i] = metricsNames[i] switch
				{
					"fscore" => new FScore(dataset),
					"rule-count" => new RuleCount(),

					_ => throw new ArgumentException($"Unknown metric: {metricsNames[i]}."),
				};
			}

			return metrics;
		}
	}
}
