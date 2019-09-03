namespace Minotaur {

	/// <summary>
	/// Since all values in a <see cref="Minotaur.Collections.Dataset.Dataset"/>
	/// are stored as floats, including values of categorical features,
	/// this is enum is used to indicate how a dataset column (representing a feature)
	/// should be treated.
	/// 
	/// The value <see cref="FeatureType.Categorical"/> indicates that
	/// the feature has a categorical nature (e.g. hair color) 
	/// and must be treated as such.
	/// For instance, two values from a column marked with
	/// <see cref="FeatureType.Categorical"/>
	/// should not be compared with the "less than operator".
	/// </summary>
	public enum FeatureType {
		Categorical,
		Continuous
	}
}
