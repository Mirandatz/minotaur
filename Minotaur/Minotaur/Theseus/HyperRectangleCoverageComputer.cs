namespace Minotaur.Theseus {
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCoverageComputer {
		private readonly Dataset _dataset;
		private readonly IConcurrentCache<HyperRectangle, DatasetCoverage> _cache;

		public HyperRectangleCoverageComputer(Dataset dataset, IConcurrentCache<HyperRectangle, DatasetCoverage> cache) {
			_dataset = dataset;
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
			var instanceCount = _dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instaceIsCovered.Length; i++) {
				var instanceData = _dataset.GetInstanceData(i);
				instaceIsCovered[i] = hyperRectangle.Contains(instanceData);
			}

			return new DatasetCoverage(
				dataset: _dataset,
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
