// Slappergen Model
#pragma warning disable 414,649
using System;
using Slapper.Attributes;

namespace Slapper.Tests.Model
{
	[SlapperEntity("Employee")]
	public partial class Employee
	{
		#region ID
		[SlapperField("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		protected Int32 _ID_Value;
		[SlapperFieldModifier("ID")]
		protected bool _ID_Modified;
		[SlapperIgnore]
		public virtual Int32 ID
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
		[SlapperField("Name")]
		protected String _Name_Value;
		[SlapperFieldModifier("Name")]
		protected bool _Name_Modified;
		[SlapperIgnore]
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
		[SlapperField("CompanyID")]
		protected Int32 _CompanyID_Value;
		[SlapperFieldModifier("CompanyID")]
		protected bool _CompanyID_Modified;
		[SlapperIgnore]
		public virtual Int32 CompanyID
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
		#region ShiftStart
		[SlapperField("ShiftStart")]
		protected TimeSpan? _ShiftStart_Value;
		[SlapperFieldModifier("ShiftStart")]
		protected bool _ShiftStart_Modified;
		[SlapperIgnore]
		public virtual TimeSpan? ShiftStart
		{
			get
			{
				return _ShiftStart_Value;
			}
			set
			{
				if (!_ShiftStart_Modified)
					_ShiftStart_Modified = _ShiftStart_Value != value;
				_ShiftStart_Value = value;
			}
		}
		#endregion
	}
	[SlapperEntity("Company")]
	public partial class Company
	{
		#region ID
		[SlapperField("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		protected Int32 _ID_Value;
		[SlapperFieldModifier("ID")]
		protected bool _ID_Modified;
		[SlapperIgnore]
		public virtual Int32 ID
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
		[SlapperField("Name")]
		protected String _Name_Value;
		[SlapperFieldModifier("Name")]
		protected bool _Name_Modified;
		[SlapperIgnore]
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
		[SlapperField("IndexName", FieldFlags.ReadOnly)]
		protected String _IndexName_Value;
		[SlapperFieldModifier("IndexName")]
		protected bool _IndexName_Modified;
		[SlapperIgnore]
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
