namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class SeedSelector {
		public readonly Dataset Dataset;
		private readonly HyperRectangleCreator _featureSpaceRegionCreator;
		private readonly RuleCoverageComputer _ruleCoverageComputer;

		public SeedSelector(
			Dataset dataset,
			HyperRectangleCreator featureSpaceRegionCreator,
			RuleCoverageComputer ruleCoverageComputer
			) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_featureSpaceRegionCreator = featureSpaceRegionCreator ?? throw new ArgumentNullException(nameof(featureSpaceRegionCreator));
			_ruleCoverageComputer = ruleCoverageComputer ?? throw new ArgumentNullException(nameof(ruleCoverageComputer));
		}

		public bool TryFindSeed(Array<Rule> existingRules, out Array<float> seed) {
			if (existingRules == null)
				throw new ArgumentNullException(nameof(existingRules));
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			if (existingRules.IsEmpty) {
				var datasetInstanceIndex = Random.Int(
					inclusiveMin: 0,
					exclusiveMax: Dataset.InstanceCount);

				seed = Dataset.GetInstanceData(datasetInstanceIndex);
				return true;
			}

			var totalCoverage = FindCombinedRuleCoverage(existingRules);
			var potentialSeeds = totalCoverage.IndicesOfUncoveredInstances;

			if (potentialSeeds.Length == 0) {
				seed = null;
				return false;
			} else {
				var datasetInstanceIndex = Random.Choice(potentialSeeds);
				seed = Dataset.GetInstanceData(datasetInstanceIndex);
				return true;
			}
		}

		private RuleCoverage FindCombinedRuleCoverage(Array<Rule> existingRules) {
			var coverages = new RuleCoverage[existingRules.Length];
			Parallel.For(0, coverages.Length, i => {
				coverages[i] = _ruleCoverageComputer.ComputeRuleCoverage(existingRules[i]);
			});

			var totalCoverage = RuleCoverage.CombineCoveragesBinaryOr(coverages);
			return totalCoverage;
		}
	}
}
