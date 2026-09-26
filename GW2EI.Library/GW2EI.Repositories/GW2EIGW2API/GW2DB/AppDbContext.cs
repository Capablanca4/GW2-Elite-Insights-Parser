using System.Reflection;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GW2EIGW2API.GW2DB;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GW2APISkill> Skills => Set<GW2APISkill>();
    public DbSet<GW2APISpec> Specs => Set<GW2APISpec>();
    public DbSet<GW2APIMap> Maps => Set<GW2APIMap>();
    public DbSet<GW2APITrait> Traits => Set<GW2APITrait>();
    public DbSet<GW2APiBuild> Build => Set<GW2APiBuild>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in GetDbSetEntityTypes())
        {
            foreach (var property in GetJsonProperties(entityType))
            {
                ConfigureJsonProperty(modelBuilder, entityType, property);
            }
        }
    }

    private IEnumerable<Type> GetDbSetEntityTypes()
    {
        return GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0]);
    }

    private static IEnumerable<PropertyInfo> GetJsonProperties(Type entityType)
    {
        return entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<JsonColumnAttribute>() is not null);
    }

    private static void ConfigureJsonProperty(
        ModelBuilder modelBuilder,
        Type entityType,
        PropertyInfo property)
    {
        typeof(AppDbContext)
            .GetMethod(nameof(MapProperty), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(property.PropertyType)
            .Invoke(null, [modelBuilder, entityType, property.Name]);
    }

    private static void MapProperty<TList>(ModelBuilder modelBuilder, Type entityType, string propertyName)
        where TList : class
    {
        modelBuilder.Entity(entityType)
            .Property<TList>(propertyName)
            .HasConversion(
                v => Serialize(v),
                s => Deserialize<TList>(s),
                new ValueComparer<TList>(
                    (a, b) => Serialize(a) == Serialize(b),
                    v => Serialize(v).GetHashCode(),
                    v => Deserialize<TList>(Serialize(v))));
    }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static string Serialize<TList>(TList? list) where TList : class?
    {
        return JsonSerializer.Serialize(list, Options);
    }

    private static TList Deserialize<TList>(string json) where TList : class?
    {
        return JsonSerializer.Deserialize<TList>(json, Options)!;
    }
}
