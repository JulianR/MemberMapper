using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MemberMapper.Core.Interfaces
{
  public interface IProposedMemberMapping
  {
    MemberInfo From { get; set; }
    MemberInfo To { get; set; }
  }
}
