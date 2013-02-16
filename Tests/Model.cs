// Slappergen
using System;
using System.Collections.Generic;
using Slapper.Attributes;

namespace Slapper.Tests.Model
{
	[Entity("Company", true)]
	public partial class Company
	{
		[Field("ID")]
		private Int32 _ID_Value;
		[Modified("ID")]
		private bool _ID_Modified;
		[Ignore]
		public virtual Int32 ID
		{
			get { return _ID_Value; }
			set {
				if (_ID_Value != value)
					_ID_Modified = true;
				_ID_Value = value;
			}
		}
		[Field("Name")]
		private String _Name_Value;
		[Modified("Name")]
		private bool _Name_Modified;
		[Ignore]
		public virtual String Name
		{
			get { return _Name_Value; }
			set {
				if (_Name_Value != value)
					_Name_Modified = true;
				_Name_Value = value;
			}
		}
	}
	[Entity("Employee", true)]
	public partial class Employee
	{
		[Field("ID")]
		private Int32 _ID_Value;
		[Modified("ID")]
		private bool _ID_Modified;
		[Ignore]
		public virtual Int32 ID
		{
			get { return _ID_Value; }
			set {
				if (_ID_Value != value)
					_ID_Modified = true;
				_ID_Value = value;
			}
		}
		[Field("Name")]
		private String _Name_Value;
		[Modified("Name")]
		private bool _Name_Modified;
		[Ignore]
		public virtual String Name
		{
			get { return _Name_Value; }
			set {
				if (_Name_Value != value)
					_Name_Modified = true;
				_Name_Value = value;
			}
		}
		[Field("CompanyID")]
		private Int32 _CompanyID_Value;
		[Modified("CompanyID")]
		private bool _CompanyID_Modified;
		[Ignore]
		public virtual Int32 CompanyID
		{
			get { return _CompanyID_Value; }
			set {
				if (_CompanyID_Value != value)
					_CompanyID_Modified = true;
				_CompanyID_Value = value;
			}
		}
	}
}
