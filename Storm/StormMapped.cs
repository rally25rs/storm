using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Storm.Attributes;
using System.Reflection.Emit;

namespace Storm
{
	/// <summary>
	/// Classes mapped by Storm can optionally extend this class to provide convenience methods.
	/// In addition to implementing IStormMapped (primarily by calling static methods on StormMapper),
	///  this abstract class also provides a facility for determining if the object has changed, and
	///  needs to be updated. This means that using .Persist() can be more efficient, since the
	///  query will only be run if the object actually changed. If StormMapped is not extended,
	///  calling StormMapper.Persist() will always run a query to update the data store, even
	///  if there are no actual changes.
	/// However, that also means more memory is used, since a copy of the original needs to be
	///  maintained for comparison purposes (I think Hibernate does the same thing; mutable
	///  object are duplicated in memory).
	/// </summary>
	public abstract class StormMapped : IStormMapped
	{
		// will hold the values of mutable properties for later comparison. keyed on property name.
        protected List<object> _stormTrackedPropertyValues { get; set; }
        private DynamicMethod saveValues;
        private DynamicMethod checkValues;

        public StormMapped()
        {
            this.GenerateDynamicMethods();
        }

        /// <summary>
        /// This method builds a pair of <see cref="DynamicMethod"/>s that can be used to save off
        ///  the current values of the mapped properties, and later compare them to the current
        ///  property values to see if any have changed.  The values are stored in an array and are
        ///  always stored in the same order, that way we don't have to use the overhead of a Hash,
        ///  and keeps the saveValues and checkValues methods at O(n) where n is the numebr of
        ///  mapped properties.
        /// </summary>
		private void GenerateDynamicMethods()
		{
            if (this._stormTrackedPropertyValues == null)
                this._stormTrackedPropertyValues = new List<object>();

            // generate dynamic methods to save and check all property values.
            // this used to be done by reflection, but this is much faster...
            this.saveValues = new DynamicMethod("", null, new Type[] { this.GetType() }, this.GetType());
            this.checkValues = new DynamicMethod("", typeof(bool), new Type[] { this.GetType() }, this.GetType());
            ILGenerator sv_il = this.saveValues.GetILGenerator();
            ILGenerator cv_il = this.checkValues.GetILGenerator();
            Label cv_retTrueLabel = cv_il.DefineLabel();
 
            MethodInfo get_values = typeof(StormMapped).GetProperty("_stormTrackedPropertyValues", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);
            MethodInfo add_value = this._stormTrackedPropertyValues.GetType().GetMethod("Add");
            MethodInfo get_value = this._stormTrackedPropertyValues.GetType().GetMethod("get_Item");

            int propertyNumber = 0;
			ClassLevelMappedAttribute[] classAttribArr = this.GetType().GetCachedAttributes<ClassLevelMappedAttribute>(true);
			foreach (ClassLevelMappedAttribute classAttrib in classAttribArr)
			{
				if ((classAttrib.SupressEvents & (StormPersistenceEvents.Insert | StormPersistenceEvents.Update)) != (StormPersistenceEvents.Insert | StormPersistenceEvents.Update))
				{
                    // Do NOT itterate through ClassLevelMappedAttribute.PropertyAttributes here.
                    // If we call this method from the constructor, then that collection will not
                    // have been populated yet, because it happens in the Validate methods, which
                    // aren't run untill first load/persist.
					foreach (PropertyInfo pi in this.GetType().GetProperties())
					{
                        PropertyLevelMappedAttribute[] propAttribArr = pi.GetCachedAttributes<PropertyLevelMappedAttribute>(true);
                        foreach (PropertyLevelMappedAttribute propAttrib in propAttribArr)
                        {
                            if (!(propAttrib is StormRelationMappedAttribute) && (classAttrib.SupressEvents & (StormPersistenceEvents.Insert | StormPersistenceEvents.Update)) != (StormPersistenceEvents.Insert | StormPersistenceEvents.Update))
                            {
                                /* *** IL to save property values *** */

                                // load 'this._stormTrackedPropertyValues' onto the stack
                                sv_il.Emit(OpCodes.Ldarg_0);
                                sv_il.Emit(OpCodes.Call, get_values);

                                // get the value of the property onto the stack
                                sv_il.Emit(OpCodes.Ldarg_0);
                                sv_il.Emit(OpCodes.Call, pi.GetGetMethod(true));

                                // box a value type into a ref type if needed
                                // ex: int -> Int32, bool -> Boolean, etc...
                                if (pi.PropertyType.IsValueType)
                                    sv_il.Emit(OpCodes.Box, pi.PropertyType);

                                // add the return value from the property's getter into the value list
                                sv_il.Emit(OpCodes.Callvirt, add_value);

                                /* *** IL to check property values *** */

                                // load 'this._stormTrackedPropertyValues' onto the stack
                                cv_il.Emit(OpCodes.Ldarg_0);
                                cv_il.Emit(OpCodes.Call, get_values);

                                // load the value out of the saved values array onto the stack
                                cv_il.Emit(OpCodes.Ldc_I4, propertyNumber);
                                cv_il.Emit(OpCodes.Callvirt, get_value);

                                // unbox value
                                if (pi.PropertyType.IsValueType)
                                    cv_il.Emit(OpCodes.Unbox_Any, pi.PropertyType);

                                // get the value of the property onto the stack
                                cv_il.Emit(OpCodes.Ldarg_0);
                                cv_il.Emit(OpCodes.Call, pi.GetGetMethod(true));

                                // see if the values are equal. if not, branch to label.
                                cv_il.Emit(OpCodes.Ceq); // puts int value 0 or 1 on the stack
                                cv_il.Emit(OpCodes.Brfalse, cv_retTrueLabel);

                                propertyNumber++;
                            }
                        }
					}
				}
			}
            // return void
            sv_il.Emit(OpCodes.Ret);

            // return false
            cv_il.Emit(OpCodes.Ldc_I4_0);
            cv_il.Emit(OpCodes.Ret);
            // return true;
            cv_il.MarkLabel(cv_retTrueLabel);
            cv_il.Emit(OpCodes.Ldc_I4_1);
            cv_il.Emit(OpCodes.Ret);
		}

		#region IStormMapped Members

		/// <summary>
		/// Whether or not any properties on this object have been changed that need to be persisted.
		/// This property will automatically be checked by the .Persist() methods, so you do not need
		///  to check this prior to calling .Persist().
		/// </summary>
		public bool HasChangesToPersist
		{
			get
			{
				if (this.checkValues == null)
					return false;

				try
				{
                    return (bool)this.checkValues.Invoke(this, new object[] { this });
                }
				catch(Exception e)
				{
					throw new StormPersistenceException("Error determining if the object of type [" + this.GetType().FullName + "] has changes to persist.", e);
				}
			}
		}

		public void Load(IDbConnection connection)
		{
			StormMapper.Load(this, connection);
		}

		public void Load(IDbConnection connection, bool cascade)
		{
			StormMapper.Load(this, connection, cascade);
		}

		public void Persist(IDbConnection connection)
		{
			StormMapper.Persist(this, connection);
		}

		public void Persist(IDbConnection connection, bool cascade)
		{
			StormMapper.Persist(this, connection, cascade);
		}

		public void Delete(IDbConnection connection)
		{
			StormMapper.Delete(this, connection);
		}

		public void Delete(IDbConnection connection, bool cascade)
		{
			StormMapper.Delete(this, connection, cascade);
		}

		/// <summary>
		/// Save a copy of all properties decorated with a <see cref="PropertyLevelMappedAttribute"/> that are insert/updateable.
		/// </summary>
		public void StormLoaded()
		{
            this.saveValues.Invoke(this, new object[] { this });
		}

		#endregion
	}
}
