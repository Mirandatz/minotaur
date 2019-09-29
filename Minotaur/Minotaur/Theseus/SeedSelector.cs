namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class SeedSelector {
		public readonly Dataset Dataset;
		private readonly RuleCoverageComputer _ruleCoverageComputer;

		public SeedSelector(
			HyperRectangleCreator hyperRectangleCreator,
			RuleCoverageComputer ruleCoverageComputer
			) {
			_ruleCoverageComputer = ruleCoverageComputer ?? throw new ArgumentNullException(nameof(ruleCoverageComputer));

			Dataset = hyperRectangleCreator.Dataset;
		}

		public bool TryFindSeed(Array<Rule> existingRules, out int datasetInstanceIndex) {
			if (existingRules == null)
				throw new ArgumentNullException(nameof(existingRules));
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			if (existingRules.IsEmpty) {
				datasetInstanceIndex = Random.Int(
					inclusiveMin: 0,
					exclusiveMax: Dataset.InstanceCount);

				return true;
			}

			var totalCoverage = FindCombinedRuleCoverage(existingRules);
			var potentialSeeds = totalCoverage.IndicesOfUncoveredInstances;

			if (potentialSeeds.Length == 0) {
				datasetInstanceIndex = default;
				return false;
			} else {
				datasetInstanceIndex = Random.Choice(potentialSeeds);
				return true;
			}
		}

		private DatasetCoverage FindCombinedRuleCoverage(Array<Rule> existingRules) {
			var coverages = new DatasetCoverage[existingRules.Length];
			Parallel.For(0, coverages.Length, i => {
				coverages[i] = _ruleCoverageComputer.ComputeRuleCoverage(existingRules[i]);
			});

			var totalCoverage = DatasetCoverage.CombineCoveragesBinaryOr(coverages);
			return totalCoverage;
		}
	}
}
