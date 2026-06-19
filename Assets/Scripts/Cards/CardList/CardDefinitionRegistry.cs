using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Cards.CardList
{
    public static class CardDefinitionRegistry
    {
        private static Dictionary<string, CardDefinition> _definitions;

        public static IReadOnlyDictionary<string, CardDefinition> Definitions
        {
            get
            {
                EnsureLoaded();
                return _definitions;
            }
        }

        public static IEnumerable<string> AllIds => Definitions.Keys;

        public static CardDefinition GetDefinition(string id)
        {
            EnsureLoaded();
            return _definitions[id];
        }

        public static bool TryGetDefinition(string id, out CardDefinition definition)
        {
            EnsureLoaded();
            return _definitions.TryGetValue(id, out definition);
        }

        public static Card CreateCard(string id, string uniqueId = null)
        {
            return GetDefinition(id).CreateCard(uniqueId);
        }

        private static void EnsureLoaded()
        {
            if (_definitions != null)
                return;

            _definitions = new Dictionary<string, CardDefinition>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type != null).ToArray();
                }

                foreach (Type type in types)
                {
                    if (type == null || type.IsAbstract || !typeof(CardDefinition).IsAssignableFrom(type))
                        continue;

                    CardDefinitionAttribute attribute =
                        type.GetCustomAttributes(typeof(CardDefinitionAttribute), false)
                            .OfType<CardDefinitionAttribute>()
                            .FirstOrDefault();

                    if (attribute == null)
                        continue;

                    if (Activator.CreateInstance(type) is not CardDefinition definition)
                        continue;

                    definition.Id = attribute.Id;
                    if (_definitions.ContainsKey(definition.Id))
                    {
                        Debug.LogError($"Duplicate card definition id '{definition.Id}' on {type.FullName}.");
                        continue;
                    }

                    _definitions.Add(definition.Id, definition);
                }
            }
        }
    }
}
