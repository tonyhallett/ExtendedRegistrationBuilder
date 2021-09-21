using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;
using System.Reflection.Context;

namespace ExtendedRegistrationBuilder
{
    public class RegistrationBuilderWithMethods : CustomReflectionContext
    {
        private readonly RegistrationBuilderDefault registrationBuilder = new RegistrationBuilderDefault();
        private readonly List<PartBuilderWithMethods> partBuilders = new List<PartBuilderWithMethods>();
        private readonly Dictionary<MethodInfo, List<Attribute>> methodAttributes = new Dictionary<MethodInfo, List<Attribute>>();
        private readonly List<MemberInfo> builtTypes = new List<MemberInfo>();
        protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
        {
            var normal = registrationBuilder.DefaultGetCustomAttributes(member, declaredAttributes);
            if((member.MemberType == MemberTypes.NestedType || member.MemberType == MemberTypes.TypeInfo) && !builtTypes.Contains(member))
            {
                builtTypes.Add(member);
                foreach (var partBuilder in partBuilders.Where(pb => pb.SelectType((Type)member))){
                    partBuilder.BuildMethodAttributes((Type)member, methodAttributes);
                }
            }
            if (member.MemberType == MemberTypes.Method)
            {
                if(!methodAttributes.TryGetValue(member as MethodInfo, out var attributes))
                {
                    attributes = new List<Attribute> { };
                }
                return normal.Concat(attributes);
            }
            if(member.MemberType == MemberTypes.Property)
            {
                var pName = (member as PropertyInfo).Name;

            }
            return normal;
        }

        public PartBuilder<T> ForTypesDerivedFrom<T>()
        {
            return registrationBuilder.ForTypesDerivedFrom<T>();
        }

        public PartBuilder<T> ForType<T>()
        {
            return registrationBuilder.ForType<T>();
        }

        public PartBuilder<T> ForTypesMatching<T>(Predicate<Type> typeFilter)
        {
            return registrationBuilder.ForTypesMatching<T>(typeFilter);
        }

        public PartBuilderWithMethods ForTypesDerivedFrom(Type type)
        {
            return new PartBuilderWithMethods(registrationBuilder.ForTypesDerivedFrom(type), (t) => type != t && type.IsAssignableFrom(t));
        }

        public PartBuilderWithMethods ForType(Type type)
        {
            var partBuilder =  new PartBuilderWithMethods(registrationBuilder.ForType(type), (t) => t == type);
            partBuilders.Add(partBuilder);
            return partBuilder;
        }

        public PartBuilderWithMethods ForTypesMatching(Predicate<Type> typeFilter)
        {
            var partBuilder = new PartBuilderWithMethods(registrationBuilder.ForTypesMatching(typeFilter),typeFilter);
            partBuilders.Add(partBuilder);
            return partBuilder;
        }
    }

}
