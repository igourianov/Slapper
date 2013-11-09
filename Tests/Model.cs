// Slappergen Model
#pragma warning disable 414,649
using Slapper.Attributes;

namespace Slapper.Tests.Model
{
	[Entity("Employee")]
	public partial class Employee
	{
		#region ID
		[Field("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		protected int? _ID_Value;
		[Ignore, Modifier("ID")]
		protected bool _ID_Modified;
		[Ignore]
		public virtual int? ID
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
		[Field("Name")]
		protected string _Name_Value;
		[Ignore, Modifier("Name")]
		protected bool _Name_Modified;
		[Ignore]
		public virtual string Name
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
		[Field("CompanyID")]
		protected int? _CompanyID_Value;
		[Ignore, Modifier("CompanyID")]
		protected bool _CompanyID_Modified;
		[Ignore]
		public virtual int? CompanyID
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
		[Field("ShiftStart")]
		protected System.TimeSpan? _ShiftStart_Value;
		[Ignore, Modifier("ShiftStart")]
		protected bool _ShiftStart_Modified;
		[Ignore]
		public virtual System.TimeSpan? ShiftStart
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
	[Entity("Company")]
	public partial class Company
	{
		#region ID
		[Field("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity)]
		protected int? _ID_Value;
		[Ignore, Modifier("ID")]
		protected bool _ID_Modified;
		[Ignore]
		public virtual int? ID
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
		[Field("Name")]
		protected string _Name_Value;
		[Ignore, Modifier("Name")]
		protected bool _Name_Modified;
		[Ignore]
		public virtual string Name
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
		[Field("IndexName", FieldFlags.ReadOnly)]
		protected string _IndexName_Value;
		[Ignore, Modifier("IndexName")]
		protected bool _IndexName_Modified;
		[Ignore]
		public virtual string IndexName
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
