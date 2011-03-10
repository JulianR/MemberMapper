using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Interfaces;
using System.Reflection;

namespace MemberMapper.Core.Implementations
{
  public class ProposedTypeMapping : IProposedTypeMapping
  {
    public PropertyOrFieldInfo SourceMember { get; set; }
    public PropertyOrFieldInfo DestinationMember { get; set; }

    public ProposedTypeMapping()
    {
      ProposedMappings = new List<IProposedMemberMapping>();
      ProposedTypeMappings = new List<IProposedTypeMapping>();
    }

    public bool IsEnumerable { get; set; }

    public IList<IProposedTypeMapping> ProposedTypeMappings { get; set; }

    public IList<IProposedMemberMapping> ProposedMappings { get; set; }

    public ProposedTypeMapping Clone()
    {
      return new ProposedTypeMapping
      {
        DestinationMember = this.DestinationMember,
        SourceMember = this.SourceMember,
        ProposedMappings = this.ProposedMappings,
        ProposedTypeMappings = this.ProposedTypeMappings
      };
    }

    public override bool Equals(object obj)
    {
      var other = obj as ProposedTypeMapping;

      if (other == null) return false;

      return Equals((ProposedTypeMapping)obj);
    }

    public bool Equals(ProposedTypeMapping mapping)
    {
      return this.DestinationMember == mapping.DestinationMember && this.SourceMember== mapping.SourceMember;
    }
  }
}
