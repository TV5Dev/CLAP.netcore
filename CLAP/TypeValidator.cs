﻿using System.Collections;
using System.Diagnostics;
using System.Reflection;
#if !FW2
using System.Linq;
#endif

namespace CLAP
{
    internal static class TypeValidator
    {
        public static void Validate(object obj)
        {
            if (Ignore(obj))
            {
                return;
            }

            Debug.Assert(obj != null);

            var type = obj.GetType();

            // type's validators:
            // first use all the validators defined over this type
            //
            var validators = type.
                GetAttributes<CollectionValidationAttribute>().
                Select(a => a.GetValidator());

            var properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var propsAndValues = properties.
                Where(p => p.GetIndexParameters().None()).
                Select(p => new ValueInfo(p.Name, p.PropertyType, p.GetValue(obj, null))).
                ToArray();

            foreach (var validator in validators)
            {
                validator.Validate(propsAndValues);
            }

            // no need to validate properties of GAC objects
            //
#if !NETCORE
            if (!type.Assembly.GlobalAssemblyCache)
#endif
            if (!type.IsArray)
            {
                // property validators:
                // validate each property value, in case property is a custom class
                //
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj, null);

                    // single validators
                    //
                    var propertySingleValidators = property.
                        GetAttributes<ValidationAttribute>().
                        Select(a => a.GetValidator());

                    foreach (var propertySingleValidator in propertySingleValidators)
                    {
                        propertySingleValidator.Validate(new ValueInfo(property.Name, property.PropertyType, value));
                    }

                    // no need to validate primitives etc with collection validators
                    //
                    if (!Ignore(value))
                    {
                        // collection validators
                        //
                        var propertyCollectionValidators = property.
                            GetAttributes<CollectionValidationAttribute>().
                            Select(a => a.GetValidator());

                        foreach (var propertyValidator in propertyCollectionValidators)
                        {
                            var propertyPropsAndValues = value.GetType().GetTypeInfo().
                                GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).
                                Where(p => p.GetIndexParameters().None()).
                                Select(p => new ValueInfo(p.Name, p.PropertyType, p.GetValue(value, null))).
                                ToArray();

                            propertyValidator.Validate(propertyPropsAndValues);
                        }
                    }
                }

                // recursion:
                // validate all values, in case their type has additional validation
                //
                foreach (var value in propsAndValues.Select(p => p.Value))
                {
                    Validate(value);
                }
            }

            // IEnumerable
            //
            var collection = obj as IEnumerable;

            if (collection != null)
            {
                foreach (var item in collection)
                {
                    Validate(item);
                }
            }
        }

        private static bool Ignore(object obj)
        {
            if (obj == null ||
                obj is string)
            {
                return true;
            }

            if (obj is IEnumerable)
            {
                return false;
            }

            var type = obj.GetType();

            if (type.IsArray ||
#if !NETCORE
                type.Assembly.GlobalAssemblyCache ||
#endif
                type.GetTypeInfo().IsEnum)
            {
                return true;
            }


            return false;
        }
    }
}