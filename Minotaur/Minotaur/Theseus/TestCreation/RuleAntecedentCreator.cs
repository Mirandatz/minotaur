namespace Minotaur.Theseus.TestCreation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class RuleAntecedentCreator {
		public Dataset Dataset { get; }

		public RuleAntecedentCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IFeatureTest[] CreateAntecedent(ReadOnlySpan<int> indicesOfInstances) {
			if (indicesOfInstances.Length <= 1)
				throw new ArgumentException();

			if (Dataset.FeatureTypes.Any(v => v != FeatureType.Continuous))
				throw new NotImplementedException();

			// @Todo: add safety / sanity checks
			var featureCount = Dataset.FeatureCount;
			var intervals = new IDimensionInterval[featureCount];

			return CreateFromIntervals(intervals);
		}

		private IFeatureTest[] CreateFromIntervals(IDimensionInterval[] intervals) {
			var featureTypes = Dataset.FeatureTypes;
			var tests = new IFeatureTest[featureTypes.Length];

			for (int i = 0; i < tests.Length; i++) {

			}

			return tests;
		}
	}
}
