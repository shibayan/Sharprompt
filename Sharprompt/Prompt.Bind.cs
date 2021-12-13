﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Sharprompt.Forms;
using Sharprompt.Internal;

namespace Sharprompt
{
    public static partial class Prompt
    {
        public static T Bind<T>() where T : new()
        {
            var model = new T();

            return Bind(model);
        }

        public static T Bind<T>(T model)
        {
            StartBind(model);

            return model;
        }

        private static void StartBind<T>(T model)
        {
            var propertyMetadatas = PropertyMetadataFactory.Create(model);

            foreach (var propertyMetadata in propertyMetadatas)
            {
                var formType = propertyMetadata.DetermineFormType();

                var result = formType switch
                {
                    FormType.Confirm => MakeConfirm(propertyMetadata),
                    FormType.Input => MakeInput(propertyMetadata),
                    FormType.List => MakeList(propertyMetadata),
                    FormType.MultiSelect => MakeMultiSelect(propertyMetadata),
                    FormType.Password => MakePassword(propertyMetadata),
                    FormType.Select => MakeSelect(propertyMetadata),
                    _ => null
                };

                propertyMetadata.PropertyInfo.SetValue(model, result);
            }
        }

        private static bool MakeConfirm(PropertyMetadata propertyMetadata)
        {
            return Confirm(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValue = (bool?)propertyMetadata.DefaultValue;
            });
        }

        private static object MakeInput(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeInputCore), propertyMetadata);

        private static T MakeInputCore<T>(PropertyMetadata propertyMetadata)
        {
            return Input<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValue = propertyMetadata.DefaultValue;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeList(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeListCore), propertyMetadata, propertyMetadata.ElementType);

        private static IEnumerable<T> MakeListCore<T>(PropertyMetadata propertyMetadata)
        {
            return List<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.DefaultValues = (IEnumerable<T>)propertyMetadata.DefaultValue;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeMultiSelect(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeMultiSelectCore), propertyMetadata, propertyMetadata.ElementType);

        private static IEnumerable<T> MakeMultiSelectCore<T>(PropertyMetadata propertyMetadata)
        {
            return MultiSelect<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.Items = propertyMetadata.ItemsProvider?.GetItems<T>();
                options.DefaultValues = (IEnumerable<T>)propertyMetadata.DefaultValue;
            });
        }

        private static string MakePassword(PropertyMetadata propertyMetadata)
        {
            return Password(options =>
            {
                options.Message = propertyMetadata.Message;

                options.Validators.Merge(propertyMetadata.Validators);
            });
        }

        private static object MakeSelect(PropertyMetadata propertyMetadata) => InvokeMethod(nameof(MakeSelectCore), propertyMetadata);

        private static T MakeSelectCore<T>(PropertyMetadata propertyMetadata)
        {
            return Select<T>(options =>
            {
                options.Message = propertyMetadata.Message;
                options.Items = propertyMetadata.ItemsProvider?.GetItems<T>();
                options.DefaultValue = propertyMetadata.DefaultValue;
            });
        }

        private static object InvokeMethod(string name, PropertyMetadata propertyMetadata, Type genericType = default)
        {
            var method = typeof(Prompt).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                                       .MakeGenericMethod(genericType ?? propertyMetadata.Type);

            return method.Invoke(null, new object[] { propertyMetadata });
        }
    }
}
