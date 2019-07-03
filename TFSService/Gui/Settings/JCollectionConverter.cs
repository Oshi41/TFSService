using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Gui.Settings
{
    /// <summary>
    ///     Конвертер для <see cref="ObservableCollection" /> свойств.
    /// </summary>
    /// <typeparam name="TSource">Тип элемента в коллекции</typeparam>
    /// <typeparam name="TConcrete">Конкретный тип класса для десереализации</typeparam>
    internal class JCollectionConverter<TSource, TConcrete> : JsonConverter
        where TConcrete : TSource
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ICollection).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            //var gen = typeof(List<>);
            //var instanceType = gen.MakeGenericType(objectType);

            //var instance = instanceType.GetConstructor(null).Invoke(null);

            //var methoInfo = instanceType.GetMethod("Add");

            //methoInfo.Invoke(instance, new object [] { 1 });
            //gen.GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance, );
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