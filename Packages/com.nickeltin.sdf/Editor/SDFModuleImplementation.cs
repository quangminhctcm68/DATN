using System;
using nickeltin.Core.Editor;

namespace nickeltin.SDF.Editor
{
    public class SDFModuleImplementation : ModuleImplementation
    {
        public override Type DefinitionType => typeof(SDFModuleDefinition);
    }
}