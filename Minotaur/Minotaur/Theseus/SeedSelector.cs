namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class SeedSelector {
		public readonly Dataset Dataset;
		private readonly HyperRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;

		public SeedSelector(
			HyperRectangleCreator hyperRectangleCreator,
			HyperRectangleCoverageComputer hyperRectangleCoverageComputer
			) {
			_boxCreator = hyperRectangleCreator;
			_coverageComputer = hyperRectangleCoverageComputer;
			Dataset = hyperRectangleCreator.Dataset;
		}

		public bool TryFindSeed(Array<Rule> existingRules, out int datasetInstanceIndex) {
			if (existingRules.ContainsNulls())
				throw new ArgumentException(nameof(existingRules) + " can't contain nulls.");

			if (existingRules.IsEmpty) {
				datasetInstanceIndex = Random.Int(
					inclusiveMin: 0,
					exclusiveMax: Dataset.InstanceCount);

				return true;
			}

			var rectangles = _boxCreator.FromRules(existingRules);
			var coverages = _coverageComputer.ComputeCoverages(rectangles);
			var totalCoverage = DatasetCoverage.CombineCoveragesBinaryOr(coverages);
			var potentialSeeds = totalCoverage.IndicesOfUncoveredInstances;

			if (potentialSeeds.Length == 0) {
				datasetInstanceIndex = default;
				return false;
			} else {
				datasetInstanceIndex = Random.Choice(potentialSeeds);
				return true;
			}
		}
	}
}
