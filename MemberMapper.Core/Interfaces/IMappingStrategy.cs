using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemberMapper.Core.Implementations;

namespace MemberMapper.Core.Interfaces
{

  public delegate void MappingOptions(PropertyOrFieldInfo source, PropertyOrFieldInfo destination, IMappingOption option);

  public interface IMappingStrategy
  {
    IProposedMap CreateMap(TypePair pair, MappingOptions options = null);

    IProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null);

    IMemberMap CreateAndFinalizeMap(TypePair pair, MappingOptions options = null);

    event Action<IMemberMap> MemberMapCreated;
  }
}
