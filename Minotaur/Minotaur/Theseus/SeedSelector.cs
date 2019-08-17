namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class SeedSelector {
		private readonly Dataset _dataset;
		private readonly FeatureSpaceRegionCreator _featureSpaceRegionCreator;
		private readonly RuleCoverageComputer _ruleCoverageComputer;

		public SeedSelector(
			Dataset dataset,
			FeatureSpaceRegionCreator featureSpaceRegionCreator, 
			RuleCoverageComputer ruleCoverageComputer
			) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_featureSpaceRegionCreator = featureSpaceRegionCreator ?? throw new ArgumentNullException(nameof(featureSpaceRegionCreator));
			_ruleCoverageComputer = ruleCoverageComputer ?? throw new ArgumentNullException(nameof(ruleCoverageComputer));
		}

		public bool TryFindSeed(Array<Rule> existingRules, out FeatureSpaceRegion seed) {
			if (existingRules == null)
				throw new ArgumentNullException(nameof(existingRules));
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			var totalCoverage = FindCombinedRuleCoverage(existingRules);
			var potentialSeeds = totalCoverage.IndicesOfUncoveredInstances;

			if (potentialSeeds.Length == 0) {
				seed = null;
				return false;
			}

			var datasetInstanceIndex = Random.Choice(potentialSeeds);

			seed = _featureSpaceRegionCreator.FromDatasetInstance(datasetInstanceIndex);
			return true;
		}

		private RuleCoverage FindCombinedRuleCoverage(Array<Rule> existingRules) {
			var coverages = new RuleCoverage[existingRules.Length];
			Parallel.For(0, coverages.Length, i => {
				coverages[i] = _ruleCoverageComputer.ComputeRuleCoverage(existingRules[i]);
			});

			var totalCoverage = RuleCoverage.Or(coverages);
			return totalCoverage;
		}
	}
}
