// Slappergen Model
#pragma warning disable 414,649
using System;
using Slapper.Attributes;

namespace Slapper.Tests.Model
{
	[Entity("Employee")]
	public partial class Employee
	{
		#region ID
		[Field("ID", FieldFlags.None | FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		private Int32? _ID_Value;
		[Modified("ID")]
		private bool _ID_Modified;
		[Ignore]
		public virtual Int32? ID
		{
			get
			{
				return _ID_Value;
			}
			set
			{
				if (!_ID_Modified)
					_ID_Modified = _ID_Value != value;
				_ID_Value = value;
			}
		}
		#endregion

		#region Name
		[Field("Name", FieldFlags.None)]
		private String _Name_Value;
		[Modified("Name")]
		private bool _Name_Modified;
		[Ignore]
		public virtual String Name
		{
			get
			{
				return _Name_Value;
			}
			set
			{
				if (!_Name_Modified)
					_Name_Modified = _Name_Value != value;
				_Name_Value = value;
			}
		}
		#endregion

		#region CompanyID
		[Field("CompanyID", FieldFlags.None)]
		private Int32? _CompanyID_Value;
		[Modified("CompanyID")]
		private bool _CompanyID_Modified;
		[Ignore]
		public virtual Int32? CompanyID
		{
			get
			{
				return _CompanyID_Value;
			}
			set
			{
				if (!_CompanyID_Modified)
					_CompanyID_Modified = _CompanyID_Value != value;
				_CompanyID_Value = value;
			}
		}
		#endregion

	}
	[Entity("Company")]
	public partial class Company
	{
		#region ID
		[Field("ID", FieldFlags.None | FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		private Int32? _ID_Value;
		[Modified("ID")]
		private bool _ID_Modified;
		[Ignore]
		public virtual Int32? ID
		{
			get
			{
				return _ID_Value;
			}
			set
			{
				if (!_ID_Modified)
					_ID_Modified = _ID_Value != value;
				_ID_Value = value;
			}
		}
		#endregion

		#region Name
		[Field("Name", FieldFlags.None)]
		private String _Name_Value;
		[Modified("Name")]
		private bool _Name_Modified;
		[Ignore]
		public virtual String Name
		{
			get
			{
				return _Name_Value;
			}
			set
			{
				if (!_Name_Modified)
					_Name_Modified = _Name_Value != value;
				_Name_Value = value;
			}
		}
		#endregion

		#region IndexName
		[Field("IndexName", FieldFlags.None | FieldFlags.ReadOnly)]
		private String _IndexName_Value;
		[Modified("IndexName")]
		private bool _IndexName_Modified;
		[Ignore]
		public virtual String IndexName
		{
			get
			{
				return _IndexName_Value;
			}
			set
			{
				if (!_IndexName_Modified)
					_IndexName_Modified = _IndexName_Value != value;
				_IndexName_Value = value;
			}
		}
		#endregion

	}
}
