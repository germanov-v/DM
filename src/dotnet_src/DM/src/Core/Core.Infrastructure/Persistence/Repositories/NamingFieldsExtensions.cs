using System.Reflection;
using System.Text;
using Dapper;

namespace Core.Infrastructure.Persistence.Repositories;

public static class NamingFieldsExtensions
{
    // TODO: кодогенерация
   public static void MapDapper()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        var assembly = Assembly.GetExecutingAssembly();

        var types = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(ISqlFields).IsAssignableFrom(t));

        foreach (var t in types)
        {
            SqlMapper.SetTypeMap(t, new SnakeCaseTypeMap(t));
        }
    }

     private sealed class SnakeCaseTypeMap : SqlMapper.ITypeMap
    {
        private readonly Type _type;
        private readonly ConstructorInfo? _ctor;
        private readonly ParameterInfo[] _ctorParams;
        private readonly Dictionary<string, ParameterInfo> _ctorParamBySnake;
        private readonly Dictionary<string, PropertyInfo> _propBySnake;

        public SnakeCaseTypeMap(Type type)
        {
            _type = type;

            // позицонный ктор
            _ctor = _type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                         .OrderByDescending(c => c.GetParameters().Length)
                         .FirstOrDefault();

            _ctorParams = _ctor?.GetParameters() ?? Array.Empty<ParameterInfo>();
            _ctorParamBySnake = _ctorParams
                .GroupBy(p => ToSnakeCase(p.Name!))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            _propBySnake = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite) // на случай классов с set ?
                .GroupBy(p => ToSnakeCase(p.Name))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        // 1) Dapper спросит: какой конструктор использовать?
        public ConstructorInfo? FindConstructor(string[] names, Type[] types)
        {
            if (_ctor is null) return null;

             var snakeSet = new HashSet<string>(names.Select(n => n), StringComparer.OrdinalIgnoreCase);

            // все параметры должны присутствовать (по snake_case)
            foreach (var p in _ctorParams)
            {
                var snake = ToSnakeCase(p.Name!);
                if (!snakeSet.Contains(snake))
                    return null; // не все найдены — пусть Dapper попробует свой дефолтный путь
            }

            return _ctor;
        }

        // eсли Dapper не смог по сигнатуре, он позовёт без типов
        public ConstructorInfo? FindExplicitConstructor() => _ctor;

        // Сопоставление параметра конструктора по имени колонки
        public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            var snake = columnName;
            if (_ctorParamBySnake.TryGetValue(snake, out var p))
                return new SimpleMemberMap(columnName, p);

            // попробуем ещё раз с конверсией (на случай если Dapper прислал PascalCase)
            snake = ToSnakeCase(columnName);
            if (_ctorParamBySnake.TryGetValue(snake, out p))
                return new SimpleMemberMap(columnName, p);

            return null;
        }

        // cопоставление свойства по имени колонки (fallback, если нет подходящего конструктора)
        public SqlMapper.IMemberMap? GetMember(string columnName)
        {
            if (_propBySnake.TryGetValue(columnName, out var prop))
                return new SimpleMemberMap(columnName, prop);

            var snake = ToSnakeCase(columnName);
            if (_propBySnake.TryGetValue(snake, out prop))
                return new SimpleMemberMap(columnName, prop);

            // last hope — обычный поиск по имени свойства
            var direct = _type.GetProperty(columnName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return direct is null ? null : new SimpleMemberMap(columnName, direct);
        }

        //  GuidId -> guid_id, URLValue -> url_value, ID -> id
        private static string ToSnakeCase(string name)
        {
            return ToSnakeCaseStack(name);
            if (string.IsNullOrEmpty(name)) return name;

            var sb = new StringBuilder(name.Length + 8);
            var prevLower = false;

            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                var isUpper = char.IsUpper(c);

                if (i > 0 && isUpper && prevLower)
                    sb.Append('_');

                // "HTTPServer" -> "http_server"
                if (i > 0 && isUpper && i + 1 < name.Length && char.IsLower(name[i + 1]))
                    sb.Append('_');

                sb.Append(char.ToLowerInvariant(c));
                prevLower = char.IsLetter(c) && !isUpper;
            }

            return sb.ToString();
        }
        
        private static string ToSnakeCaseStack(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

          
            Span<char> buffer = stackalloc char[name.Length + 8];
            var pos = 0;
            var prevLower = false;

            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                var isUpper = char.IsUpper(c);

                if (i > 0 && isUpper && prevLower)
                    buffer[pos++] = '_';
                //"HTTPServer" -> "http_server"
                else if (i > 0 && isUpper && i + 1 < name.Length && char.IsLower(name[i + 1]))
                    buffer[pos++] = '_';

                buffer[pos++] = char.ToLowerInvariant(c);
                prevLower = char.IsLetter(c) && !isUpper;
            }

            return new string(buffer[..pos]); 
        }

        private sealed class SimpleMemberMap : SqlMapper.IMemberMap
        {
            public string ColumnName { get; }
            public Type MemberType { get; }
            public PropertyInfo? Property { get; }
            public FieldInfo? Field => null;
            public ParameterInfo? Parameter { get; }

            public SimpleMemberMap(string columnName, PropertyInfo property)
            {
                ColumnName = columnName;
                Property = property;
                MemberType = property.PropertyType;
            }

            public SimpleMemberMap(string columnName, ParameterInfo parameter)
            {
                ColumnName = columnName;
                Parameter = parameter;
                MemberType = parameter.ParameterType;
            }
        }
    }
}

internal interface ISqlFields
{
}