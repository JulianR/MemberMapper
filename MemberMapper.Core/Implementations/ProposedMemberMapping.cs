using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Reflection;

namespace MemberMapper.Core.Implementations
{
  public class ProposedMemberMapping : IProposedMemberMapping
  {
    public PropertyOrFieldInfo From { get; set; }
    public PropertyOrFieldInfo To { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is ProposedMemberMapping)) return false;

      if (obj == null) return false;

      return Equals((ProposedMemberMapping)obj);
    }

    public bool Equals(ProposedMemberMapping mapping)
    {
      return this.To == mapping.To && this.From == mapping.From;
    }

    public override int GetHashCode()
    {
      return this.To.GetHashCode() ^ this.From.GetHashCode();
    }

    public bool IsEnumerable
    {
      get;
      set;
    }
  }
}
