namespace Minotaur.Classification {

	/// <summary>
	/// Since all values in a <see cref="Collections.Dataset.Dataset"/>
	/// are stored as floats, including values of categorical features,
	/// this is enum is used to indicate how a dataset column (representing a feature)
	/// should be treated.
	/// For a miriad of reasons, categorical attributes must be one-hot-encoded.
	public enum FeatureType {
		NotImplemented,
		Continuous
	}
}
