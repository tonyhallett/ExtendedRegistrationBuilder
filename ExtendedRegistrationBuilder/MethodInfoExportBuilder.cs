using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;

namespace ExtendedRegistrationBuilder
{
    public sealed class MethodInfoExportBuilder
    {
        private bool _isInherited;
        private string _contractName;
        private Type _contractType;
        private List<Tuple<string, object>> _metadataItems;
        private List<Tuple<string, Func<MethodInfo, object>>> _metadataItemFuncs;

        public MethodInfoExportBuilder() { }

        public MethodInfoExportBuilder AsContractType<T>()
        {
            return AsContractType(typeof(T));
        }

        public MethodInfoExportBuilder AsContractType(Type type)
        {
            _contractType = type;
            return this;
        }

        public MethodInfoExportBuilder AsContractName(string contractName)
        {
            _contractName = contractName;
            return this;
        }

        public MethodInfoExportBuilder Inherited()
        {
            _isInherited = true;
            return this;
        }

        public MethodInfoExportBuilder AddMetadata(string name, object value)
        {
            if (_metadataItems == null)
            {
                _metadataItems = new List<Tuple<string, object>>();
            }
            _metadataItems.Add(Tuple.Create(name, value));

            return this;
        }

        public MethodInfoExportBuilder AddMetadata(string name, Func<MethodInfo, object> itemFunc)
        {
            if (_metadataItemFuncs == null)
            {
                _metadataItemFuncs = new List<Tuple<string, Func<MethodInfo, object>>>();
            }
            _metadataItemFuncs.Add(Tuple.Create(name, itemFunc));

            return this;
        }

        public void BuildAttributes(MethodInfo methodInfo, ref List<Attribute> attributes)
        {
            if (attributes == null)
            {
                attributes = new List<Attribute>();
            }

            if (_isInherited)
            {
                // Default export
                attributes.Add(new InheritedExportAttribute(_contractName, _contractType));
            }
            else
            {
                // Default export
                attributes.Add(new ExportAttribute(_contractName, _contractType));
            }

            //Add metadata attributes from direct specification
            if (_metadataItems != null)
            {
                foreach (Tuple<string, object> item in _metadataItems)
                {
                    attributes.Add(new ExportMetadataAttribute(item.Item1, item.Item2));
                }
            }

            //Add metadata attributes from func specification
            if (_metadataItemFuncs != null)
            {
                foreach (Tuple<string, Func<MethodInfo, object>> item in _metadataItemFuncs)
                {
                    string name = item.Item1;
                    object value = (item.Item2 != null) ? item.Item2(methodInfo) : null;
                    attributes.Add(new ExportMetadataAttribute(name, value));
                }
            }
        }
    }

}
