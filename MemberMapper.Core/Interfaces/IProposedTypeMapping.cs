using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MemberMapper.Core.Implementations;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedTypeMapping
  {
    PropertyOrFieldInfo SourceMember { get; set; }
    PropertyOrFieldInfo DestinationMember { get; set; }

    bool IsEnumerable { get; set; }

    IList<IProposedMemberMapping> ProposedMappings { get; }
    IList<IProposedTypeMapping> ProposedTypeMappings { get; }
  }
}
