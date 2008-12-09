using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Storm.Attributes;

namespace TestApp.Classes
{
	[StormTableMapped(TableName="BICYCLES", DataBinder="oledbBinder")]
	class Bicycle
	{
		[StormColumnMapped(ColumnName="NAME", PrimaryKey=true)]
		public string Name { get; set; }

		[StormColumnMapped(ColumnName="FRAME")]
		public string Frame { get; set; }

		[StormColumnMapped(ColumnName="FORK")]
		public string Fork { get; set; }

		//[StormRelationMapped(column="wheelset_manuf", relatedType="MyDomain.WheelSet, MyDomain", relatedProperty="Manufacturer")]
		//[StormRelationMapped(column="wheelset_model", relatedType="MyDomain.WheelSet, MyDomain", relatedProperty="ModelName")]
		//public Wheelset Wheelset { get; set; }
		
		//[StormRelationMapped(column="name", relatedType="MyDomain.Event, MyDomain", relatedProperty="BikeUsed")]
		//public List<Event> UsedAt { get; set; }
	}
}
