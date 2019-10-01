namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class InstanceCoveringRuleAntecedentCreator {

		public Dataset Dataset { get; }

		public InstanceCoveringRuleAntecedentCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IFeatureTest[] CreateAntecedent(Array<float> seed, ReadOnlySpan<int> nearestInstancesIndices) {
			throw new NotImplementedException();
			//var box = MutableHyperRectangle.FromDatasetInstance(
			//	seed: seed,
			//	featureTypes: Dataset.FeatureTypes);

			//for (int i = 0; i < nearestInstancesIndices.Length; i++) {
			//	var index = nearestInstancesIndices[i];
			//	EnlargeRectangleToCovertInstance(
			//		mutableHyperRectangle: box,
			//		instanceIndex: index);
			//}

			//var featureCount = Dataset.FeatureCount;
			//var tests = new IFeatureTest[featureCount];
			//for (int i = 0; i < tests.Length; i++) {
			//	var dimension = box.GetDimensionInterval(i);
			//	tests[i] = _testCreator.FromDimensionInterval(dimension);
			//}

			//return tests;
		}

		//private void EnlargeRectangleToCovertInstance(MutableHyperRectangle mutableHyperRectangle, int instanceIndex) {
		//	var instance = Dataset.GetInstanceData(instanceIndex);
		//	var dimensionCount = mutableHyperRectangle.DimensionCount;

		//	for (int i = 0; i < dimensionCount; i++) {
		//		var instanceValue = instance[i];
		//		var dimension = mutableHyperRectangle.GetDimensionInterval(i);
		//		var enlargedDimension = EnlargeDimensionToContainValue(dimension, instanceValue);
		//		mutableHyperRectangle.SetDimensionInterval(enlargedDimension);
		//	}
		//}

		private IDimensionInterval EnlargeDimensionToContainValue(IDimensionInterval dimension, float instanceValue) {
			return dimension switch
			{
				BinaryDimensionInterval bdi => EnlargeBinaryDimensionInterval(bdi, instanceValue),
				ContinuousDimensionInterval cdi => EnlargeContinuousDimensionInterval(cdi, instanceValue),

				_ => throw CommonExceptions.UnknownDimensionIntervalImplementation
			};
		}

		private BinaryDimensionInterval EnlargeBinaryDimensionInterval(BinaryDimensionInterval bdi, float instanceValue) {
			if (instanceValue == 0f) {
				if (bdi.ContainsFalse)
					return bdi;

				return new BinaryDimensionInterval(
					dimensionIndex: bdi.DimensionIndex,
					containsFalse: true,
					containsTrue: bdi.ContainsTrue);
			}

			if (instanceValue == 1f) {
				if (bdi.ContainsTrue)
					return bdi;

				return new BinaryDimensionInterval(
					dimensionIndex: bdi.DimensionIndex,
					containsFalse: bdi.ContainsFalse,
					containsTrue: true);
			}

			throw new InvalidOperationException();
		}

		private ContinuousDimensionInterval EnlargeContinuousDimensionInterval(ContinuousDimensionInterval cdi, float instanceValue) {
			throw new NotImplementedException();

			//var dimensionIndex = cdi.DimensionIndex;

			//var start = cdi.Start;
			//if (instanceValue < start.Value) {
			//	var newStart = DimensionBound.CreateStart(instanceValue);
			//	return new ContinuousDimensionInterval(
			//		dimensionIndex: dimensionIndex,
			//		start: newStart,
			//		end: cdi.End);
			//}

			//var end = cdi.End;
			//if (instanceValue >= end.Value) {
			//	var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex: dimensionIndex);
			//	var indexOfInstanceValue = featureValues.BinarySearch(instanceValue);

			//	// @Sanity check
			//	if (indexOfInstanceValue < 0)
			//		throw new InvalidOperationException();

			//	if (indexOfInstanceValue < featureValues.Length - 1) {
			//		var nextValue = featureValues[indexOfInstanceValue + 1];
			//		var newEnd = DimensionBound.CreateEnd(nextValue);
			//		return new ContinuousDimensionInterval(
			//			dimensionIndex: dimensionIndex,
			//			start: cdi.Start,
			//			end: newEnd);
			//	} else {
			//		// If we reached this point, it means indexOfFeatureInstanceValue == featureValues.Length - 1
			//		var newEnd = DimensionBound.CreateEnd(float.PositiveInfinity);
			//		return new ContinuousDimensionInterval(
			//			dimensionIndex: dimensionIndex,
			//			start: cdi.Start,
			//			end: newEnd);
			//	}
			//}

			//// If we reached this point, it is because cdi must contain the instance value
			//if (!cdi.Contains(instanceValue))
			//	throw new InvalidOperationException();

			//return cdi;
		}
	}
}