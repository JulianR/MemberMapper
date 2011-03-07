using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemberMapper.Core.Interfaces
{
  public interface IMapGenerator
  {
    Delegate GenerateMappingFunction(IProposedMap map);
  }
}
