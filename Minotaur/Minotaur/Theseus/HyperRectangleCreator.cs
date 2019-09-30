namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCreator {
		public readonly Dataset Dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;
		private readonly IConcurrentCache<Rule, HyperRectangle> _cache;

		public HyperRectangleCreator(
			DimensionIntervalCreator dimensionIntervalCreator,
			IConcurrentCache<Rule, HyperRectangle> cache
			) {
			_dimensionIntervalCreator = dimensionIntervalCreator;
			_cache = cache;
			Dataset = dimensionIntervalCreator.Dataset;
		}

		/// <summary>
		/// To understand what the heck is going on,
		/// check this https://arxiv.org/abs/1908.09652
		/// </summary>
		public HyperRectangle CreateLargestNonIntersectingHyperRectangle(
			Array<float> seed,
			Array<HyperRectangle> existingRectangles,
			NaturalRange dimensionExpansionOrder
			) {

			// @Todo: add loads of safety checks
			if (existingRectangles.IsEmpty)
				return CreateMaximalRectangle();

			var mutable = MutableHyperRectangle.FromDatasetInstance(
				seed: seed,
				featureTypes: Dataset.FeatureTypes);

			throw new NotImplementedException();

			//for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
			//	var dimensionIndex = dimensionExpansionOrder[i];
				//switch (Dataset.GetFeatureType(dimensionIndex)) {

				//case FeatureType.Categorical:
				//EnlargeCategoricalDimension(
				//	seed: seed,
				//	mutable: mutable,
				//	dimensionIndex: dimensionIndex,
				//	existingRectangles: existingRectangles);
				//break;

				//case FeatureType.CategoricalButTriviallyValued:
				//// Ain't nothing to do, bruh... There is a single
				//// value in the dataset... Whata pain...
				//break;

				//case FeatureType.Continuous:
				//EnlargeContinuousDimension(
				//	seed: seed,
				//	mutable: mutable,
				//	dimensionIndex: dimensionIndex,
				//	existingRectangles: existingRectangles);
				//break;

				//case FeatureType.ContinuousButTriviallyValued:
				//EnlargeContinuousDimension(
				//	seed: seed,
				//	mutable: mutable,
				//	dimensionIndex: dimensionIndex,
				//	existingRectangles: existingRectangles);
				//break;

				//default:
				//throw new InvalidOperationException(ExceptionMessages.UnknownFeatureType);
				//}
			//}

			//return mutable.ToHyperRectangle();
		}

		private HyperRectangle CreateMaximalRectangle() {
			throw new NotImplementedException();
			//var dimensions = new IDimensionInterval[Dataset.FeatureCount];
			//for (int i = 0; i < dimensions.Length; i++)
			//	dimensions[i] = _dimensionIntervalCreator.CreateMaximalDimensionInterval(dimensionIndex: i);

			//return new HyperRectangle(dimensions);
		}

		private void EnlargeCategoricalDimension(
			Array<float> seed,
			MutableHyperRectangle mutable,
			int dimensionIndex,
			Array<HyperRectangle> existingRectangles
			) {

			throw new NotImplementedException();

			//var possibleValues = Dataset
			//	.GetSortedUniqueFeatureValues(featureIndex: dimensionIndex)
			//	.ToHashSet();

			//for (int i = 0; i < existingRectangles.Length; i++) {
			//	var otherRectangle = existingRectangles[i];

			//	// In the lords name, the most basic of sanity checks
			//	//if (otherRectangle.Contains(seed))
			//	//	throw new InvalidOperationException();

			//	var intersects = HyperRectangleIntersector.IntersectsInAllButOneDimension(
			//		target: mutable,
			//		other: otherRectangle,
			//		dimensionToSkip: dimensionIndex);

			//	var otherDimension = (CategoricalDimensionInterval) otherRectangle.GetDimensionInterval(dimensionIndex);

			//	if (intersects)
			//		possibleValues.ExceptWith(otherDimension.SortedValues);

			//	// It's not possible to enlarge the dimension
			//	if (possibleValues.Count == 0)
			//		return;
			//}

			//var sortedPossibleValues = possibleValues
			//	.OrderBy(v => v)
			//	.ToArray();

			//var enlargedDimension = CategoricalDimensionInterval.FromSortedUniqueValues(
			//	dimensionIndex: dimensionIndex,
			//	sortedUniqueValues: sortedPossibleValues);

			//mutable.SetDimensionInterval(enlargedDimension);
		}

		private void EnlargeContinuousDimension(
			Array<float> seed,
			MutableHyperRectangle mutable,
			int dimensionIndex,
			Array<HyperRectangle> existingRectangles
			) {

			// @Assumption: continuous feature may have
			// any have from negative infinity to positive infinity
			var min = float.NegativeInfinity;
			var max = float.PositiveInfinity;

			for (int i = 0; i < existingRectangles.Length; i++) {
				var otherRectangle = existingRectangles[i];

				// In the lords name, the most basic of sanity checks
				if (otherRectangle.Contains(seed))
					throw new InvalidOperationException();

				var intersects = HyperRectangleIntersector.IntersectsInAllButOneDimension(
					target: mutable,
					other: otherRectangle,
					dimensionToSkip: dimensionIndex);

				if (!intersects)
					continue;

				var otherDimension = (ContinuousDimensionInterval) otherRectangle.Dimensions[dimensionIndex];

				var seedValue = seed[dimensionIndex];
				var otherMin = otherDimension.Start.Value;
				var otherMax = otherDimension.End.Value;

				if (seedValue >= otherMax)
					min = Math.Max(min, otherMax);
				else
					max = Math.Min(max, otherMin);

				//// If the seed is to the right of the other interval
				//// we need to adjust the left side of our enlarged rectangle,
				//// i.e. the min
				//if (seedValue > otherMax)
				//	min = Math.Max(min, otherMax);

				//// If the seed is to the left of the other interval,
				//// we need to adjust the right side of our enlarged rectangle,
				//// i.e. the max
				//if (seedValue <= otherMin)
				//	max = Math.Min(max, otherMin);
			}

			// @Assumption all continuous intervals have 
			// inclusive start values and exclusive end values
			var start = DimensionBound.CreateStart(min);
			var end = DimensionBound.CreateEnd(max);
			var enlargedDimension = new ContinuousDimensionInterval(
				dimensionIndex: dimensionIndex,
				start: start,
				end: end);

			mutable.SetDimensionInterval(enlargedDimension);
		}

		public HyperRectangle FromRule(Rule rule) {
			if (_cache.TryGet(rule, out var hyperRectangle)) {
				return hyperRectangle;
			} else {
				hyperRectangle = UnchachedFromRule(rule);
				_cache.Add(key: rule, value: hyperRectangle);
				return hyperRectangle;
			}
		}

		public HyperRectangle[] FromRules(Array<Rule> rules) {
			var hyperRectangles = new HyperRectangle[rules.Length];

			Parallel.For(0, hyperRectangles.Length, i => {
				var currentRule = rules[i];
				var hyperRectangle = FromRule(currentRule);
				hyperRectangles[i] = hyperRectangle;
			});

			return hyperRectangles;
		}

		private HyperRectangle UnchachedFromRule(Rule rule) {
			var tests = rule.Tests;
			var dimensions = new IDimensionInterval[tests.Length];

			for (int i = 0; i < dimensions.Length; i++)
				dimensions[i] = _dimensionIntervalCreator.FromFeatureTest(tests[i]);

			return new HyperRectangle(dimensions: dimensions);
		}
	}
}
