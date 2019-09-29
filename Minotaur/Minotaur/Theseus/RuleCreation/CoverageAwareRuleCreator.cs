namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly int _minimumInstancesToCover;

		public CoverageAwareRuleCreator(Dataset dataset, SeedSelector seedSelector, HyperRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer) {
			Dataset = dataset;
			_seedSelector = seedSelector;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
		}

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule rule) {
			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				datasetInstanceIndex: out var seedIndex);

			if (!seedFound) {
				rule = null!;
				return false;
			}

			var seed = Dataset.GetInstanceData(seedIndex);
			var existingRectangles = _boxCreator.FromRules(existingRules);

			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var secureRectangle = _boxCreator.CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: existingRectangles,
				dimensionExpansionOrder: dimensionExpansionOrder);

			if (!secureRectangle.Contains(seed))
				throw new InvalidOperationException();

			var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);

			// @Consideration: maybe we could try finding another seed?
			if(secureRectangleCoverage.IndicesOfCoveredInstances.Length < _minimumInstancesToCover) {
				rule = null!;
				return false;
			}

			throw new NotImplementedException();
		}
	}
}
