using HelixToolkit.Wpf.SharpDX;
using MCCS.Core.Models.Model3D;
using SharpDX;

namespace MCCS.Common
{
    public class EnumToMaterial
    {

        public static Material GetMaterialFromEnum(MaterialEnum materialType)
        {
            return materialType switch
            {
                MaterialEnum.Original => new PhongMaterial
                {
                    DiffuseColor = Color.Gray,
                    SpecularColor = Color.White,
                    SpecularShininess = 32,
                },
                MaterialEnum.Selected => new PhongMaterial
                {
                    DiffuseColor = Color.Yellow,
                    SpecularColor = Color.White,
                    SpecularShininess = 64,
                    EmissiveColor = Color.Yellow * 0.3f
                },
                MaterialEnum.Hover => new PhongMaterial
                {
                    DiffuseColor = Color.LightBlue,
                    SpecularColor = Color.White,
                    SpecularShininess = 48
                },
                _ => new PhongMaterial
                {
                    DiffuseColor = Color.Gray,
                    SpecularColor = Color.White,
                    SpecularShininess = 32,
                }
            };
        }

    }
}
