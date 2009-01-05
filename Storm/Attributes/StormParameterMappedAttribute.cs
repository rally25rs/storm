using System.Data;
using System.Reflection;

namespace Storm.Attributes
{
	/// <summary>
	/// This attribute indicates that the decorated property
	///  is to be mapped to a procedure parameter by Storm.
	/// </summary>
	public class StormParameterMappedAttribute : PropertyLevelMappedAttribute
	{
		public string ParameterName { get; set; }
		public ParameterDirection ParameterDirection { get; set; }

		public StormParameterMappedAttribute()
			: base()
		{
		}

		internal override void ValidateMapping(PropertyInfo decoratedProperty)
		{
			// base validation
			base.ValidateMapping(decoratedProperty);
			this.Validated = false;

			// must have a column name defined.
			// if no name specified, then reuse the Parameter name.
			if (this.ParameterName == null || this.ParameterName.Length == 0)
				this.ParameterName = decoratedProperty.Name;

			this.Validated = true;
		}
	}
}
