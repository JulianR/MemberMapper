using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedTypeMapping
  {
    MemberInfo SourceMember { get; set; }
    MemberInfo DestinationMember { get; set; }

    IList<IProposedMemberMapping> ProposedMappings { get; }
    IList<IProposedTypeMapping> ProposedTypeMappings { get; }

  }
}
