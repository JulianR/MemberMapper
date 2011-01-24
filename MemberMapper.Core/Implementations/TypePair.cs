using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemberMapper.Core.Implementations
{
  public class TypePair
  {
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public TypePair(Type source, Type destination)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (destination == null) throw new ArgumentNullException("destination");

      this.SourceType = source;
      this.DestinationType = destination;
    }

    public override bool Equals(object obj)
    {
      if(!(obj is TypePair)) return false;

      if (obj == null) return false;

      return Equals((TypePair)obj);
    }

    public bool Equals(TypePair other)
    {
      return this.SourceType == other.SourceType && this.DestinationType == other.DestinationType;
    }

    public override int GetHashCode()
    {
      return this.SourceType.GUID.GetHashCode() ^ this.DestinationType.GUID.GetHashCode();
    }

  }
}
