using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Core.Models.Model3D;
using Color = SharpDX.Color;

namespace MCCS.Common
{
    public class EnumToMaterial
    {
        public static Material GetMaterialFromEnum(MaterialEnum materialType)
        {
            /*
             * new PhongMaterial
                {
                    DiffuseColor = Color.LightBlue,
                    SpecularColor = Color.White,
                    SpecularShininess = 32,
                }
             */
            /*
             * var pathBase = @"F:\DiamondPlate005C_1K-PNG";
            
            var normalPath = @$"{pathBase}\Engraved_Metal_NORM.jpg";
            if (File.Exists(normalPath))
            {
                try
                {
                    var pbrMaterial = new PBRMaterial()
                    {
                        AlbedoColor = Colors.Blue.ToColor4(),
                        RenderEnvironmentMap = true,
                        AlbedoMap = TextureModel.Create(@$"{pathBase}\Engraved_Metal_COLOR.jpg"),
                        NormalMap = TextureModel.Create(@$"{pathBase}\Engraved_Metal_NORM.jpg"),
                        DisplacementMap = TextureModel.Create(@$"{pathBase}\Engraved_Metal_DISP.png"),
                        RoughnessMetallicMap = TextureModel.Create(@$"{pathBase}\Engraved_Metal_RMC.png"),
                        DisplacementMapScaleMask = new Vector4(0.1f, 0.1f, 0.1f, 0),
                        EnableAutoTangent = true,
                        EnableTessellation = true,
                        MaxDistanceTessellationFactor = 2,
                        MinDistanceTessellationFactor = 4
                    };
                    /// 检查图像信息
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(normalPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    Debug.WriteLine($"图像尺寸: {bitmap.PixelWidth}x{bitmap.PixelHeight}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"法线贴图读取错误: {ex.Message}");
                }
            }
             */
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
