namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Random = Random.ThreadStaticRandom;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		private readonly Dataset _dataset;
		private readonly CFSBESeedFinder _seedSelector;
		private readonly RuleAntecedentHyperRectangleConverter _boxConverter;
		private readonly NonIntersectingRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly AntecedentCreator _antecedentCreator;
		private readonly IConsequentCreator _consequentCreator;
		private readonly HyperRectangleIntersector _rectangleIntersector;
		private readonly int _targetNumberOfInstancesToCover;
		private readonly bool _runExpensiveSanityChecks;

		public CoverageAwareRuleCreator(CFSBESeedFinder seedSelector, RuleAntecedentHyperRectangleConverter boxConverter, NonIntersectingRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer, AntecedentCreator antecedentCreator, IConsequentCreator consequentCreator, HyperRectangleIntersector hyperRectangleIntersector, int targetNumberOfInstancesToCover, Dataset dataset, bool runExpensiveSanityChecks) {
			_seedSelector = seedSelector;
			_boxConverter = boxConverter;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
			_antecedentCreator = antecedentCreator;
			_consequentCreator = consequentCreator;
			_targetNumberOfInstancesToCover = targetNumberOfInstancesToCover;
			_rectangleIntersector = hyperRectangleIntersector;
			_dataset = dataset;
			_runExpensiveSanityChecks = runExpensiveSanityChecks;
		}

		public Rule? TryCreateRule(ReadOnlySpan<Rule> existingRules) {
			var seedsIndices = _seedSelector.FindSeedsIndices(existingRules);

			if (seedsIndices.IsEmpty)
				return null;

			var seedIndex = Random.Choice(seedsIndices);
			var seed = _dataset.GetInstanceData(seedIndex);

			var boxes = _boxConverter.FromRules(existingRules);

			if (_runExpensiveSanityChecks) {
				for (int i = 0; i < boxes.Length; i++) {
					var box = boxes[i];
					if (box.Contains(seed))
						throw new InvalidOperationException();
				}
			}

			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: _dataset.FeatureCount);

			var secureRectangle = _boxCreator.TryCreateLargestNonIntersectingRectangle(
				seedIndex: seedIndex,
				existingHyperRectangles: boxes,
				dimensionExpansionOrder: dimensionExpansionOrder);

			if (secureRectangle is null)
				return null;

			if (_runExpensiveSanityChecks) {

				for (int i = 0; i < boxes.Length; i++) {
					var box = boxes[i];
					if (_rectangleIntersector.IntersectsInAllDimension(secureRectangle, box))
						throw new InvalidOperationException();
				}
			}

			var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);
			var coveredInstancesIndices = secureRectangleCoverage.IndicesOfCoveredInstances.ToArray();

			if (coveredInstancesIndices.Length == 0)
				return null;

			var coveredInstancesDistancesToSeed = _dataset.ComputeDistances(
				targetInstanceIndex: seedIndex,
				otherInstancesIndices: coveredInstancesIndices);

			Array.Sort(
				keys: coveredInstancesDistancesToSeed,
				items: coveredInstancesIndices);

			var instancesToCover = Math.Min(_targetNumberOfInstancesToCover, coveredInstancesIndices.Length);
			var relevantInstances = coveredInstancesIndices
				.AsSpan()
				.Slice(start: 0, length: instancesToCover);

			var ruleAntecedent = _antecedentCreator.CreateAntecedent(
				seedIndex: seedIndex,
				nearestInstancesIndices: relevantInstances);

			if (ruleAntecedent is null)
				return null;

			var ruleConsequent = _consequentCreator.CreateConsequent(relevantInstances);

			return new Rule(
				antecedent: ruleAntecedent,
				consequent: ruleConsequent);
		}
	}
}
