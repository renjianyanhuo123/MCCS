using HelixToolkit.Wpf.SharpDX;
using System.Windows.Media;
using SharpDX;

namespace MCCS.Models.Model3D
{
    public enum MaterialTypeEnum : int
    {
        /// <summary>
        /// 塑料材质
        /// </summary>
        Plastic,
        /// <summary>
        /// 金属材质
        /// </summary>
        Metal,
        /// <summary>
        /// 发光材质
        /// </summary>
        Luminescence
    }

    public static class MaterialTypeEnumToMaterial
    {
        public static Material GetMaterialByEnum(MaterialTypeEnum materialType)
        {
            switch (materialType)
            {
                case MaterialTypeEnum.Plastic:
                    return new PhongMaterial()
                    {
                        DiffuseColor = Colors.Red.ToColor4(),
                        AmbientColor = Colors.Red.ToColor4() * 0.2f,
                        SpecularColor = Color4.White,
                        SpecularShininess = 32.0f,
                        EmissiveColor = Color4.Black
                    };
                case MaterialTypeEnum.Metal:
                    return new PhongMaterial()
                    {
                        DiffuseColor = Colors.Silver.ToColor4() * 0.3f,
                        AmbientColor = Colors.Silver.ToColor4() * 0.1f,
                        SpecularColor = Color4.White,
                        SpecularShininess = 128.0f,
                        EmissiveColor = Color4.Black
                    };
                case MaterialTypeEnum.Luminescence:
                    return new PhongMaterial()
                    {
                        DiffuseColor = Colors.Blue.ToColor4() * 0.1f,
                        AmbientColor = Color4.Black,
                        SpecularColor = Color4.Black,
                        SpecularShininess = 1.0f,
                        EmissiveColor = Colors.Blue.ToColor4() * 0.8f
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(materialType), materialType, null);
            }
        }
    }
}
