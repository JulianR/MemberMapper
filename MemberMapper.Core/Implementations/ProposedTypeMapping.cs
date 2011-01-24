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
    public MemberInfo SourceProperty { get; set; }
    public MemberInfo DestinationProperty { get; set; }

    public ProposedTypeMapping()
    {
      ProposedMappings = new List<IProposedMemberMapping>();
    }

    public IList<IProposedTypeMapping> ProposedTypeMappings { get; set; }

    public IList<IProposedMemberMapping> ProposedMappings { get; set; }
  }
}
