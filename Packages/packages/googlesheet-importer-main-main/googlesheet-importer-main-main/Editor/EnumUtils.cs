using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NabaGame.Core.Runtime.Extensions;

namespace NabaGame.Googlesheet.Importer.Editor
{
    public static class EnumUtils
    {
        public static List<Type> EnumTypes;

        static EnumUtils()
        {
            GetEnums();
        }

        public static void GetEnums()
        {
            List<Assembly> Assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                    assembly.GetName().Name == "Assembly-CSharp" || assembly.GetName().Name == "Assembly-CSharp-Editor")
                .ToList();
            EnumTypes = new List<Type>();
            if (!Assemblies.IsNullOrEmpty())
            {
                foreach (var assembly in Assemblies)
                {
                    EnumTypes.AddRange(assembly.GetTypes()
                        .Where(type => type.IsEnum).ToList());
                }
            }
        }

        public static Type GetEnumTypeByName(string enumName)
        {
            return EnumTypes.FirstOrDefault(x => x.FullName.Contains(enumName));
        }
    }
}