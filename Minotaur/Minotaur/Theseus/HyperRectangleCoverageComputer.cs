namespace Minotaur.Theseus {
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCoverageComputer {
		public readonly Dataset Dataset;
		private readonly IConcurrentCache<HyperRectangle, DatasetCoverage> _cache;

		public HyperRectangleCoverageComputer(Dataset dataset, IConcurrentCache<HyperRectangle, DatasetCoverage> cache) {
			Dataset = dataset;
			_cache = cache;
		}

		public DatasetCoverage ComputeCoverage(HyperRectangle hyperRectangle) {
			if (_cache.TryGet(hyperRectangle, out var coverage)) {
				return coverage;
			} else {
				coverage = UncachedComputeCoverage(hyperRectangle);
				_cache.Add(key: hyperRectangle, value: coverage);
				return coverage;
			}
		}

		private DatasetCoverage UncachedComputeCoverage(HyperRectangle hyperRectangle) {
			var instanceCount = Dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instaceIsCovered.Length; i++) {
				var instanceData = Dataset.GetInstanceData(i);
				instaceIsCovered[i] = hyperRectangle.Contains(instanceData);
			}

			return new DatasetCoverage(
				dataset: Dataset,
				instancesCovered: instaceIsCovered);
		}

		public DatasetCoverage[] ComputeCoverages(Array<HyperRectangle> hyperRectangles) {
			var coverages = new DatasetCoverage[hyperRectangles.Length];

			Parallel.For(0, hyperRectangles.Length, i => {
				var currentRectangle = hyperRectangles[i];
				var currentCoverage = ComputeCoverage(hyperRectangle: currentRectangle);
				coverages[i] = currentCoverage;
			});

			return coverages;
		}
	}
}
