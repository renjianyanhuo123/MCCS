using HelixToolkit.Wpf;
using MCCS.Models;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoader
    {
        /// <summary>
        /// 异步加载3D模型
        /// </summary>
        /// <param name="modelData">模型数据</param>
        /// <returns>加载的模型</returns>
        public static async Task<Model3D> LoadModelAsync(ModelData modelData)
        {
            return await Task.Run(() => LoadModel(modelData));
        }

        /// <summary>
        /// 加载3D模型 (同步方法，在异步方法中调用)
        /// </summary>
        private static Model3D LoadModel(ModelData modelData)
        {
            try
            {
                // 检查文件存在
                if (!File.Exists(modelData.FilePath))
                    throw new FileNotFoundException($"模型文件不存在: {modelData.FilePath}");

                Model3D model = null;

                // 根据文件扩展名选择适当的导入器
                if (modelData.FilePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
                {
                    var importer = new ObjReader();
                    model = importer.Read(modelData.FilePath);
                }
                else if (modelData.FilePath.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
                {
                    var importer = new StLReader();
                    model = importer.Read(modelData.FilePath);
                }
                else
                {
                    throw new NotSupportedException($"不支持的文件格式: {Path.GetExtension(modelData.FilePath)}");
                }

                if (model == null)
                    throw new InvalidOperationException("模型加载失败");

                // 设置模型的位置
                var transform = new TranslateTransform3D(
                    modelData.Position.X,
                    modelData.Position.Y,
                    modelData.Position.Z);
                model.Transform = transform;

                // 设置材质
                ApplyMaterialToModel(model, modelData.OriginalColor);

                return model;
            }
            catch (Exception ex)
            {
                // 记录异常，实际应用中应该使用日志记录
                System.Diagnostics.Debug.WriteLine($"模型加载异常: {ex.Message}");
                throw; // 重新抛出异常让调用者处理
            }
        }

        /// <summary>
        /// 递归设置模型材质
        /// </summary>
        private static void ApplyMaterialToModel(Model3D model, Color color)
        {
            switch (model)
            {
                case GeometryModel3D geometryModel:
                    {
                        var material = new DiffuseMaterial(new SolidColorBrush(color));
                        geometryModel.Material = material;
                        geometryModel.BackMaterial = material;
                        break;
                    }
                case Model3DGroup modelGroup:
                    {
                        foreach (var child in modelGroup.Children)
                        {
                            ApplyMaterialToModel(child, color);
                        }

                        break;
                    }
            }
        }
    }
}
