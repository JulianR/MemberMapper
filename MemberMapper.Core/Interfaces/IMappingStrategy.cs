using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;

namespace MemberMapper.Core.Interfaces
{
  public interface IMappingStrategy
  {
    IProposedMap CreateMap(TypePair pair, Action<System.Reflection.PropertyInfo, IMappingOption> options = null);

    IMemberMap CreateAndFinalizeMap(TypePair pair, Action<System.Reflection.PropertyInfo, IMappingOption> options = null);


    IMemberMapper Mapper { get; set; }
  }
}
