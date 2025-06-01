using HelixToolkit.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using MCCS.Common;
using MCCS.Core.Models.Model3D;
using MCCS.ViewModels.Others;

namespace MCCS.Services.Model3DService
{
    public class Model3DLoaderService : IModel3DLoaderService
    {
        /// <summary>
        /// 异步加载3D模型
        /// </summary>
        /// <param name="modelData">模型数据</param>
        /// <returns>加载的模型</returns>
        public async Task<Model3DViewModel> LoadModelAsync(Model3DData modelData)
        {
            return await Task.Run(() => LoadModel(modelData));
        }

        /// <summary>
        /// 异步加载3D模型
        /// </summary>
        public async Task<Model3DViewModel> LoadModel(Model3DData modelData)
        {
            // 检查文件存在
            if (!File.Exists(modelData.FilePath))
                throw new FileNotFoundException($"模型文件不存在: {modelData.FilePath}");
            var arr = modelData.PositionStr.Split(',');
            var res = new Model3DViewModel()
            {
                Id = modelData.Key,
                Name = modelData.Name,
                FilePath = modelData.FilePath,
                Position = new Point3D(Convert.ToDouble(arr[0]), Convert.ToDouble(arr[1]), Convert.ToDouble(arr[2])),
                IsInteractive = ModelType.Actuator == modelData.Type,
                IsClickable = false,
                IsSelected = false,
                IsHovered = false,
                CurrentMaterial = new DiffuseMaterial(new SolidColorBrush(modelData.OriginalColor.ToColor())),
                OriginalMaterial = new DiffuseMaterial(new SolidColorBrush(modelData.OriginalColor.ToColor()))
            };
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
                Convert.ToDouble(arr[0]),
                Convert.ToDouble(arr[1]),
                Convert.ToDouble(arr[2]));
            model.Transform = transform;
            // 设置材质
            ApplyMaterialToModel(model, res.CurrentMaterial);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                res.Model = model;
                
            }); 
            return res;
        }

        /// <summary>
        /// 递归设置模型材质
        /// </summary>
        private void ApplyMaterialToModel(Model3D model, Material material)
        {
            switch (model)
            {
                case GeometryModel3D geometryModel:
                    { 
                        geometryModel.Material = material;
                        geometryModel.BackMaterial = material;
                        break;
                    }
                case Model3DGroup modelGroup:
                    {
                        foreach (var child in modelGroup.Children)
                        {
                            ApplyMaterialToModel(child, material);
                        }

                        break;
                    }
            }
        }
    }
}
