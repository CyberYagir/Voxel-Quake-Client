using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Content.Scripts.Game.IO
{
    public class IgnorePropertiesResolver : DefaultContractResolver
    {
        private readonly HashSet<string> _propsToIgnore;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            _propsToIgnore = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (_propsToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = _ => false; // Не сериализовать
                property.Ignored = true;               // Игнорировать полностью
            }
            return property;
        }
    }
}