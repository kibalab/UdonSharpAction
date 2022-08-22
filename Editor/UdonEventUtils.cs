using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace K13A.USharpAction.Editor
{
    public class UdonEventUtils
    {
        public static System.Enum CreateEnum(List<string> list){

            System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
            AssemblyName aName = new AssemblyName("Enum");
            AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);
            EnumBuilder enumerator = mb.DefineEnum("Enum", TypeAttributes.Public, typeof(int));

            int i = 0;
            enumerator.DefineLiteral("None", i);

            foreach(string names in list){
                i++;
                enumerator.DefineLiteral(names, i);
            }

            System.Type finished = enumerator.CreateType();

            return (System.Enum)System.Enum.ToObject(finished,0);
        }
    }
}