using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gui.Settings
{
    /// <summary>
    /// Конвертер для <see cref="ObservableCollection"/> свойств.
    /// </summary>
    /// <typeparam name="TSource">Тип элемента в коллекции</typeparam>
    /// <typeparam name="TConcrete">Конкретный тип класса для десереализации</typeparam>
    class JCollectionConverter<TSource, TConcrete> : JsonConverter
        where TConcrete : TSource
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ICollection).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var readed = serializer.Deserialize<IEnumerable<TConcrete>>(reader);
            var result = new ObservableCollection<TSource>(readed.OfType<TSource>());

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
