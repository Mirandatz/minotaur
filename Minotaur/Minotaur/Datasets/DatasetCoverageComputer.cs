namespace Minotaur.Datasets {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Classification.Rules;

	public sealed class DatasetCoverageComputer {

		public readonly Dataset Dataset;

		public DatasetCoverageComputer(Dataset dataset) {
			Dataset = dataset;
		}

		public DatasetCoverage ComputeCoverage(ReadOnlySpan<Rule> rules) {
			if (rules.IsEmpty)
				throw new ArgumentException(nameof(rules) + " can't be empty.");

			var rulesArray = rules.ToArray();
			var featuresManager = Dataset.InstancesFeaturesManager;
			var instanceCount = featuresManager.InstanceCount;
			var coverageMap = new bool[instanceCount];

			// If any rule covers the "nth" dataset instance
			// we update the coverage map in the "nth" position
			// to indicate that
			Parallel.For(fromInclusive: 0, toExclusive: rulesArray.Length, body: ruleIndex => {
				var currentRuleAntecedent = rulesArray[ruleIndex].Antecedent;

				for (int i = 0; i < instanceCount; i++) {
					var instanceFeatures = featuresManager.GetFeatures(instanceIndex: i);
					if (currentRuleAntecedent.Covers(instanceFeatures))
						coverageMap[i] = true;
				}
			});

			return new DatasetCoverage(coverageMap: coverageMap);
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
