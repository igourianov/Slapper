// Slappergen Model
#pragma warning disable 414,649
using Slapper.Attributes;
using System.Diagnostics;

namespace Slapper.Tests.Model
{
	[Entity("Employee"), DebuggerNonUserCode,
	System.CodeDom.Compiler.GeneratedCodeAttribute("Slappergen", "1.0")]
	public partial class Employee
	{
		#region ID
		[Field("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected int? _ID_Value;
		[Ignore,
		Modifier("ID"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _ID_Modified;
		[Ignore]
		public virtual int? ID
		{
			[DebuggerStepThrough]
			get
			{
				return _ID_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_ID_Modified)
					_ID_Modified = _ID_Value != value;
				_ID_Value = value;
			}
		}
		#endregion
		#region Name
		[Field("Name"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected string _Name_Value;
		[Ignore,
		Modifier("Name"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _Name_Modified;
		[Ignore]
		public virtual string Name
		{
			[DebuggerStepThrough]
			get
			{
				return _Name_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_Name_Modified)
					_Name_Modified = _Name_Value != value;
				_Name_Value = value;
			}
		}
		#endregion
		#region CompanyID
		[Field("CompanyID"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected int? _CompanyID_Value;
		[Ignore,
		Modifier("CompanyID"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _CompanyID_Modified;
		[Ignore]
		public virtual int? CompanyID
		{
			[DebuggerStepThrough]
			get
			{
				return _CompanyID_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_CompanyID_Modified)
					_CompanyID_Modified = _CompanyID_Value != value;
				_CompanyID_Value = value;
			}
		}
		#endregion
		#region ShiftStart
		[Field("ShiftStart"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected System.TimeSpan? _ShiftStart_Value;
		[Ignore,
		Modifier("ShiftStart"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _ShiftStart_Modified;
		[Ignore]
		public virtual System.TimeSpan? ShiftStart
		{
			[DebuggerStepThrough]
			get
			{
				return _ShiftStart_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_ShiftStart_Modified)
					_ShiftStart_Modified = _ShiftStart_Value != value;
				_ShiftStart_Value = value;
			}
		}
		#endregion
	}
	[Entity("Company"), DebuggerNonUserCode,
	System.CodeDom.Compiler.GeneratedCodeAttribute("Slappergen", "1.0")]
	public partial class Company
	{
		#region ID
		[Field("ID", FieldFlags.Key | FieldFlags.ReadOnly | FieldFlags.Identity),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected int? _ID_Value;
		[Ignore,
		Modifier("ID"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _ID_Modified;
		[Ignore]
		public virtual int? ID
		{
			[DebuggerStepThrough]
			get
			{
				return _ID_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_ID_Modified)
					_ID_Modified = _ID_Value != value;
				_ID_Value = value;
			}
		}
		#endregion
		#region Name
		[Field("Name"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected string _Name_Value;
		[Ignore,
		Modifier("Name"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _Name_Modified;
		[Ignore]
		public virtual string Name
		{
			[DebuggerStepThrough]
			get
			{
				return _Name_Value;
			}
			[DebuggerStepThrough]
			set
			{
				if (!_Name_Modified)
					_Name_Modified = _Name_Value != value;
				_Name_Value = value;
			}
		}
		#endregion
		#region IndexName
		[Field("IndexName", FieldFlags.ReadOnly),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected string _IndexName_Value;
		[Ignore,
		Modifier("IndexName"),
		DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected bool _IndexName_Modified;
		[Ignore]
		public virtual string IndexName
		{
			[DebuggerStepThrough]
			get
			{
				return _IndexName_Value;
			}
			[DebuggerStepThrough]
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
