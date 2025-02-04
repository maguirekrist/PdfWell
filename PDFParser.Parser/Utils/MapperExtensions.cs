using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using PDFParser.Parser.Attributes;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Utils;

public static class MapperExtensions
{
    static bool IsMarkedAsNullable(PropertyInfo p)
    {
        return new NullabilityInfoContext().Create(p).WriteState is NullabilityState.Nullable;
    }
    
    public static T TryMapTo<T>(this DictionaryObject dictionaryObject)
    {
        var tProps = typeof(T).GetProperties();
        
        T target = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        
        foreach (var prop in tProps)
        {
            var dictKeyName = prop.GetCustomAttribute<PdfDictionaryKeyAttribute>()?.Key ?? prop.Name;
            var type = prop.PropertyType;
            
            //Attempt to get the value from the dictionaryObject
            var value = dictionaryObject.GetType().GetMethod("TryGetAs")?.MakeGenericMethod(type).Invoke(dictionaryObject, new object[] { dictKeyName });
            
            if (IsMarkedAsNullable(prop))
            {
                //this prop is nullable
                prop.SetValue(target, value ?? null);
            }
            else
            {
                prop.SetValue(target, value ?? throw new Exception($"required property {dictKeyName} was not found in the dictionary."));
            }
        }

        return target;
    }
}