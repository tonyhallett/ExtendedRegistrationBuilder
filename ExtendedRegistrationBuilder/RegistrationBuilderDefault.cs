using System.Collections.Generic;
using System.ComponentModel.Composition.Registration;
using System.Reflection;

namespace ExtendedRegistrationBuilder
{
    public class RegistrationBuilderDefault : RegistrationBuilder
    {
        public IEnumerable<object> DefaultGetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
        {
            return GetCustomAttributes(member, declaredAttributes);
        }
    }

}
