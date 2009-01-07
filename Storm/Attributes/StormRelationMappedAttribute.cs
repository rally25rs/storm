using System.Reflection;
using System;

namespace Storm.Attributes
{
	/// <summary>
	/// This attribute indicates that the decorated property is to be loaded
	/// from another Storm mapped type.
	/// </summary>
	public class StormRelationMappedAttribute : PropertyLevelMappedAttribute
	{
		private PropertyInfo relatedFrom = null;
		private PropertyInfo relatedTo = null;

		/// <summary>
		/// The property on the decorated type to use as the join value for the relation.
		/// </summary>
		public string Property { get; set; }

		/// <summary>
		/// The property on the related type to use to join the relation.
		/// </summary>
		public string RelatedProperty { get; set; }

		public PropertyInfo RelatedFrom
		{
			get { return this.relatedFrom; }
		}

		public PropertyInfo RelatedTo
		{
			get { return this.relatedTo; }
		}

		public StormRelationMappedAttribute()
			: base()
		{
		}

		internal override void ValidateMapping(PropertyInfo decoratedProperty)
		{
			if (this.Validated)
				return;

			// base validation
			base.ValidateMapping(decoratedProperty);
			this.Validated = false;

			// must have a Property and a RelatedProperty defined.
			if (this.Property == null || this.Property.Length == 0 || this.RelatedProperty == null || this.RelatedProperty.Length == 0)
				throw new StormConfigurationException("Invalid mapping. StormRelationMapped attribute attached to [" + decoratedProperty.Name + "] does not have its Property and/or RelatedProperty defined.");

			// check the Property and RelatedProperty names to see if they exist for the related Types.
			this.relatedFrom = decoratedProperty.DeclaringType.GetProperty(this.Property);
			Type[] generics = decoratedProperty.PropertyType.GetGenericArguments();
			if (generics != null && generics.Length == 1 && decoratedProperty.PropertyType.FullName.Contains("System.Collections.Generic.List"))
				this.relatedTo = generics[0].GetProperty(this.RelatedProperty);
			else
				this.relatedTo = decoratedProperty.PropertyType.GetProperty(this.RelatedProperty);
			if (this.relatedFrom == null)
				throw new StormConfigurationException("Invalid mapping. StormRelationMapped attribute attached to [" + decoratedProperty.Name + "] has Property set to [" + this.Property + "] but that does not exist on Type [" + decoratedProperty.DeclaringType.FullName + "].");
			if (this.relatedTo == null)
				throw new StormConfigurationException("Invalid mapping. StormRelationMapped attribute attached to [" + decoratedProperty.Name + "] has RelatedProperty set to [" + this.RelatedProperty + "] but that does not exist on Type [" + decoratedProperty.PropertyType.FullName + "].");

			this.Validated = true;
		}
	}
}
