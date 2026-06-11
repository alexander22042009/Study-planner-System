using AutoMapper;
using StudyPlanner.Infrastructure.Mapping;

namespace StudyPlanner.Tests.Helpers;

public static class TestMapperFactory
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
}
