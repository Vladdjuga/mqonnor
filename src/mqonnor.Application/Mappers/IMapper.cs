namespace mqonnor.Application.Mappers;

public interface IMapper<in TSource, out TDestination>
{
    TDestination Map(TSource source);
}
