using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;

namespace ExtendedRegistrationBuilder
{
    public class PartBuilderWithMethods
    {
        private static readonly Type s_exportAttributeType = typeof(ExportAttribute);
        private readonly List<Tuple<Predicate<MethodInfo>, Action<MethodInfo, MethodInfoExportBuilder>, Type>> _methodExports = new List<Tuple<Predicate<MethodInfo>, Action<MethodInfo, MethodInfoExportBuilder>, Type>>();
        private PartBuilder partBuilder;
        private readonly Predicate<Type> selectType;

        public PartBuilderWithMethods(PartBuilder partBuilder, Predicate<Type> selectType)
        {
            this.partBuilder = partBuilder;
            this.selectType = selectType;
        }

        public Predicate<Type> SelectType => selectType;

        #region wrappers
        public PartBuilderWithMethods Export()
        {
            partBuilder.Export();
            return this;
        }

        public PartBuilderWithMethods Export(Action<ExportBuilder> exportConfiguration)
        {
            partBuilder.Export(exportConfiguration);

            return this;
        }

        public PartBuilderWithMethods Export<T>()
        {
            partBuilder.Export<T>();
            return this;
        }

        public PartBuilderWithMethods Export<T>(Action<ExportBuilder> exportConfiguration)
        {
            partBuilder.Export<T>(exportConfiguration);

            return this;
        }

        // Choose a constructor from all of the available constructors, then configure them
        public PartBuilderWithMethods SelectConstructor(Func<ConstructorInfo[], ConstructorInfo> constructorFilter)
        {
            partBuilder.SelectConstructor(constructorFilter);
            return this;
        }

        public PartBuilderWithMethods SelectConstructor(Func<ConstructorInfo[], ConstructorInfo> constructorFilter,
            Action<ParameterInfo, ImportBuilder> importConfiguration)
        {
            partBuilder.SelectConstructor(constructorFilter, importConfiguration);
            return this;
        }

        // Choose an interface to export then configure it
        public PartBuilderWithMethods ExportInterfaces(Predicate<Type> interfaceFilter)
        {
            partBuilder.ExportInterfaces(interfaceFilter);
            return this;
        }

        public PartBuilderWithMethods ExportInterfaces()
        {
            partBuilder.ExportInterfaces();
            return this;
        }

        public PartBuilderWithMethods ExportInterfaces(Predicate<Type> interfaceFilter,
            Action<Type, ExportBuilder> exportConfiguration)
        {
            partBuilder.ExportInterfaces(interfaceFilter, exportConfiguration);

            return this;
        }

        // Choose a property to export then configure it
        public PartBuilderWithMethods ExportProperties(Predicate<PropertyInfo> propertyFilter)
        {
            partBuilder.ExportProperties(propertyFilter);
            return this;
        }

        public PartBuilderWithMethods ExportProperties(Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ExportBuilder> exportConfiguration)
        {
            partBuilder.ExportProperties(propertyFilter, exportConfiguration);

            return this;
        }

        // Choose a property to export then configure it
        public PartBuilderWithMethods ExportProperties<T>(Predicate<PropertyInfo> propertyFilter)
        {
            partBuilder.ExportProperties<T>(propertyFilter);

            return this;
        }

        public PartBuilderWithMethods ExportProperties<T>(Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ExportBuilder> exportConfiguration)
        {
            partBuilder.ExportProperties<T>(propertyFilter, exportConfiguration);

            return this;
        }

        

        // Choose a property to export then configure it
        public PartBuilderWithMethods ImportProperties(Predicate<PropertyInfo> propertyFilter)
        {
            partBuilder.ImportProperties(propertyFilter);
            return this;
        }

        public PartBuilderWithMethods ImportProperties(Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportBuilder> importConfiguration)
        {
            partBuilder.ImportProperties(propertyFilter, importConfiguration);
            return this;
        }

        // Choose a property to export then configure it
        public PartBuilderWithMethods ImportProperties<T>(Predicate<PropertyInfo> propertyFilter)
        {
            partBuilder.ImportProperties<T>(propertyFilter);
            return this;
        
        }

        public PartBuilderWithMethods ImportProperties<T>(Predicate<PropertyInfo> propertyFilter,
            Action<PropertyInfo, ImportBuilder> importConfiguration)
        {
            partBuilder.ImportProperties<T>(propertyFilter, importConfiguration);
            return this;
        }

        public PartBuilderWithMethods SetCreationPolicy(CreationPolicy creationPolicy)
        {
            partBuilder.SetCreationPolicy(creationPolicy);
            return this;
        }

        public PartBuilderWithMethods AddMetadata(string name, object value)
        {
            partBuilder.AddMetadata(name, value);
            return this;
        }

        public PartBuilderWithMethods AddMetadata(string name, Func<Type, object> itemFunc)
        {
            partBuilder.AddMetadata(name, itemFunc);

            return this;
        }
        #endregion
        
        public PartBuilderWithMethods ExportMethods(Predicate<MethodInfo> methodFilter, Action<MethodInfo,MethodInfoExportBuilder> exportConfiguration)
        {
            _methodExports.Add(Tuple.Create(methodFilter, exportConfiguration,default(Type)));
            return this;
        }
        
        internal void BuildMethodAttributes(Type type, Dictionary<MethodInfo, List<Attribute>> methodAttributes)
        {
            if (_methodExports.Any())
            {
                var methods = type.GetMethods();
                foreach (MethodInfo mi in methods)
                {
                    if (!methodAttributes.ContainsKey(mi))
                    {
                        
                        // Run through the export specifications see if any match
                        foreach (Tuple<Predicate<MethodInfo>, Action<MethodInfo, MethodInfoExportBuilder>, Type> exportSpecification in _methodExports)
                        {
                            List<Attribute> attributes = null;
                            if (exportSpecification.Item1 != null && exportSpecification.Item1(mi))
                            {
                                var exportBuilder = new MethodInfoExportBuilder();

                                if (exportSpecification.Item3 != null)
                                {
                                    exportBuilder.AsContractType(exportSpecification.Item3);
                                }

                                exportSpecification.Item2?.Invoke(mi, exportBuilder);


                                var isConfigured = mi.GetCustomAttributes(typeof(ExportAttribute), false).FirstOrDefault() != null || MemberHasExportMetadata(mi);

                                if (isConfigured)
                                {
                                    break;
                                }
                                else
                                {
                                    exportBuilder.BuildAttributes(mi, ref attributes);
                                    methodAttributes.Add(mi, attributes);
                                }
                            }
                        }

                    }

                }
            }
        }

        private static bool MemberHasExportMetadata(MemberInfo member)
        {
            foreach (Attribute attr in member.GetCustomAttributes(typeof(Attribute), false))
            {
                if (attr is ExportMetadataAttribute)
                {
                    return true;
                }
                else
                {
                    Type attrType = attr.GetType();
                    // Perf optimization, relies on short circuit evaluation, often a property attribute is an ExportAttribute
                    if (attrType != s_exportAttributeType && attrType.IsDefined(typeof(MetadataAttributeAttribute), true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }

}
