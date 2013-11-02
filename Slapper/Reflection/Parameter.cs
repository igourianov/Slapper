using System;

namespace Slapper.Reflection
{
	public interface IParameter
	{
		string Name { get; }
		Type Type { get; }
		object Value { get; }
	}

	internal class Parameter<T> : IParameter
	{
		public string Name { get; set; }
		public Type Type { get; set; }
		public T Value { get; set; }

		object IParameter.Value
		{
			get
			{
				return (object)this.Value;
			}
		}

		public Parameter(string name, Type type, T value)
		{
			this.Name = name;
			this.Type = type;
			this.Value = value;
		}
	}
}
