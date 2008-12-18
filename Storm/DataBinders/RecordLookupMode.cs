using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storm.DataBinders
{
	public enum RecordLookupMode
	{
		/// <summary>
		/// Indicates that the data for the mapping should be obtained using key fields only.
		/// Typically this means only 1 record will be found, and is used for populating a single object.
		/// For example; "get car where vin=1F01234XXX123456".
		/// </summary>
		LookupByKeys,

		/// <summary>
		/// Indicates that the data for the mapping should be obtained using any non-null properties.
		/// Typically this means 1 or more results, and is used for building lists of objects that match non-specific criteria.
		/// For example; "get cars where color=blue".
		/// </summary>
		LookupByNonNullProperties,
	}
}
