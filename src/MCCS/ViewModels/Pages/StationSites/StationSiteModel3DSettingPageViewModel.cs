using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using DryIoc;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Common;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Core.Models.Model3D;
using MCCS.Core.Models.StationSites;
using MCCS.Core.Repositories;
using MCCS.Models.Model3D;
using MCCS.Models.Stations.Model3DSettings;
using MCCS.Services.NotificationService;
using Microsoft.Win32;
using Serilog;
using SharpDX;
using Color = System.Windows.Media.Color;
using EnumToMaterial = MCCS.Common.EnumToMaterial;
using Material = HelixToolkit.Wpf.SharpDX.Material;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MCCS.ViewModels.Pages.StationSites
{
    public class StationSiteModel3DSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "StationSiteModel3DSettingPage";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;
        private readonly IModel3DDataRepository _model3DDataRepository;
        private readonly INotificationService _notificationService;
        private long _stationId = -1;
        private bool _isAdded = false;

        public StationSiteModel3DSettingPageViewModel(IEventAggregator eventAggregator, 
            IEffectsManager effectsManager,
            IStationSiteAggregateRepository stationSiteAggregateRepository,
            IModel3DDataRepository model3DDataRepository,
            INotificationService notificationService) : base(eventAggregator)
        {
            EffectsManager = effectsManager;
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
            _model3DDataRepository = model3DDataRepository;
            _notificationService = notificationService;
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
            ImportModelFileCommand = new AsyncDelegateCommand(ExecuteImportModelFileCommand);
            DeleteFileCommand = new DelegateCommand<string>(ExecuteDeleteFileCommand);
            SelectMaterialCommand = new DelegateCommand(ExecuteSelectMaterialCommand);
            SaveModelCommand = new AsyncDelegateCommand(ExecuteSaveModelCommand);
            CheckBoxSelectionChangedCommand = new DelegateCommand(ExecuteCheckBoxSelectionChangedCommand);
            AddBillboardTextCommand = new DelegateCommand(ExecuteAddBillboardTextCommand);
            TextInfoMouseDownCommand = new DelegateCommand<MouseDown3DEventArgs>(ExecuteTextInfoMouseDownCommand);
        }

        #region Property
        /// <summary>
        /// 模型整体ID
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 模型的整体名称
        /// </summary>
        private string _modelName = string.Empty;
        public string ModelName
        {
            get => _modelName;
            set => SetProperty(ref _modelName, value);
        }

        /// <summary>
        /// 模型描述
        /// </summary>
        private string _desprition = string.Empty;
        public string Desprition
        {
            get => _desprition;
            set => SetProperty(ref _desprition, value);
        }

        /// <summary>
        /// Phong材质中,漫反射颜色
        /// </summary>
        private Color _materialColor = Colors.Gray;
        public Color MaterialColor
        {
            get => _materialColor;
            set
            {
                if (SetProperty(ref _materialColor, value))
                {
                    UpdateMaterial(GroupModel.SceneNode, ColorConvertPhongMaterial(_materialColor));
                }
            }
        }

        /// <summary>
        /// 是否有模型文件
        /// </summary>
        private bool _haveData = false;
        public bool HaveData
        {
            get => _haveData;
            set => SetProperty(ref _haveData, value);
        }

        /// <summary>
        /// 相机背景色
        /// </summary>
        private Color _cameraBackground = Colors.Black;
        public Color CameraBackground
        {
            get => _cameraBackground;
            set => SetProperty(ref _cameraBackground, value);
        } 

        private HelixToolkit.Wpf.SharpDX.Camera _camera;
        public HelixToolkit.Wpf.SharpDX.Camera Camera
        {
            get => _camera; 
            set => SetProperty(ref _camera, value);
        }

        private IEffectsManager _effectsManager;
        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            set => SetProperty(ref _effectsManager, value);
        }

        public SceneNodeGroupModel3D GroupModel { get; } = new();

        private Model3DFileItemModel _selectedModel3DFile; 
        public Model3DFileItemModel SelectedModel3DFile
        {
            get => _selectedModel3DFile;
            set
            {
                if (Equals(_selectedModel3DFile, value)) return;
                if (_selectedModel3DFile != null)
                {
                    var setMaterial = EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Original) as PhongMaterial;
                    if(setMaterial != null) setMaterial.DiffuseColor = new SharpDX.Color(_materialColor.R, _materialColor.G, _materialColor.B,
                        _materialColor.A);
                    UpdateMaterial(GroupModel.SceneNode, setMaterial);  
                }

                if (value != null)
                {
                    CommonMathNode(GroupModel.SceneNode, value.Key, UpdateMaterial, EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Selected));
                    foreach (var bindedChannel in BindingControlChannels)
                    {
                        bindedChannel.IsSelected = value.BindedControlChannelIds.Any(c => c == bindedChannel.Id);
                    }
                }

                SetProperty(ref _selectedModel3DFile, value);
            }
        }

        public ObservableCollection<BindingControlChannelItemModel> BindingControlChannels { get; private set; } = [];
        
        public ObservableCollection<Model3DFileItemModel> Model3DFiles { get; private set; } = [];

        public List<BindedChannelModelBillboardTextInfo> BindedChannelModelBillboardTextInfos { get; private set; } = [];
        /// <summary>
        /// 采集数据显示的标签
        /// </summary>
        public BillboardText3D CollectionDataLabels { get; } = new()
        {
            IsDynamic = true
        };

        public LineGeometry3D ConnectionLineGeometry { get; } = new()
        {
            IsDynamic = true
        };

        #region BillboradInfo  
        private double _xDistance;
        public double XDistance
        {
            get => _xDistance;
            set
            {
                if (SetProperty(ref _xDistance, value))
                {
                    if (SelectedBillBoradInfo == null) return;
                    SelectedBillBoradInfo.XDistance = _xDistance;
                    var temp = CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin;
                    CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin =
                        new Vector3((float)_xDistance, temp.Y, temp.Z);
                    CollectionDataLabels.Invalidate();
                }
            }
        }

        private double _yDistance;
        public double YDistance
        {
            get => _yDistance;
            set
            {
                if (SetProperty(ref _yDistance, value))
                {
                    if (SelectedBillBoradInfo == null) return;
                    SelectedBillBoradInfo.YDistance = _yDistance;
                    var temp = CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin;
                    CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin =
                        new Vector3(temp.X, (float)_yDistance, temp.Z);
                    CollectionDataLabels.Invalidate();
                }
            }
        }

        private double _zDistance;
        public double ZDistance
        {
            get => _zDistance;
            set
            {
                if (SetProperty(ref _zDistance, value))
                {
                    if (SelectedBillBoradInfo == null) return;
                    SelectedBillBoradInfo.ZDistance = _zDistance;
                    var temp = CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin;
                    CollectionDataLabels.TextInfo[SelectedBillBoradInfo.Index].Origin =
                        new Vector3(temp.X, temp.Y, (float)_zDistance);
                    CollectionDataLabels.Invalidate();
                }
            }
        }

        private BindedChannelModelBillboardTextInfo _selectedBillBoradInfo;
        public BindedChannelModelBillboardTextInfo SelectedBillBoradInfo
        {
            get => _selectedBillBoradInfo;
            set => SetProperty(ref _selectedBillBoradInfo, value);
        }

        #endregion
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; }
        public AsyncDelegateCommand ImportModelFileCommand { get; }
        public DelegateCommand<string> DeleteFileCommand { get; }
        public DelegateCommand SelectMaterialCommand { get; }
        public AsyncDelegateCommand SaveModelCommand { get; }
        public DelegateCommand CheckBoxSelectionChangedCommand { get; }
        public DelegateCommand AddBillboardTextCommand { get; }
        public DelegateCommand<MouseDown3DEventArgs> TextInfoMouseDownCommand { get; }

        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _stationId = navigationContext.Parameters.GetValue<long>("StationId");
        }

        #region Private Method 
        private void ExecuteTextInfoMouseDownCommand(MouseDown3DEventArgs eventArgs)
        {
            if (eventArgs.HitTestResult.ModelHit is not BillboardTextModel3D model ||
                eventArgs.HitTestResult is not BillboardHitResult res) return;
            if (model.Geometry != CollectionDataLabels) return;
            if (res.TextInfo is not TextInfoExt clickedTextInfoModel) return;
            for (var i = 0; i < CollectionDataLabels.TextInfo.Count; i++)
            {
                CollectionDataLabels.TextInfo[i].Background = BindedChannelModelBillboardTextInfos[i].BackgroundColor;
                CollectionDataLabels.TextInfo[i].Foreground = BindedChannelModelBillboardTextInfos[i].FontColor;
            }
            clickedTextInfoModel.Background = SharpDX.Color.Yellow;
            clickedTextInfoModel.Foreground = SharpDX.Color.Black; 
            SelectedBillBoradInfo = BindedChannelModelBillboardTextInfos[res.TextInfoIndex];
            SelectedBillBoradInfo.Index = res.TextInfoIndex;
            CollectionDataLabels.Invalidate();
        }

        private static Material ColorConvertPhongMaterial(Color color)
        {
            return new PhongMaterial
            {
                DiffuseColor =
                    new SharpDX.Color(color.R, color.G, color.B, color.A),
                SpecularColor = SharpDX.Color.White,
                SpecularShininess = 32
            };
        }

        private void ExecuteCheckBoxSelectionChangedCommand()
        {
            SelectedModel3DFile.BindedControlChannelIds.Clear();
            SelectedModel3DFile.BindedControlChannelIds.AddRange(BindingControlChannels.Where(s => s.IsSelected).Select(c => c.Id));
        }

        private async Task ExecuteSaveModelCommand()
        {
            var orthographicCamera = Camera as HelixToolkit.Wpf.SharpDX.OrthographicCamera;
            var modelBaseInfo = new Model3DBaseInfo
            {
                Id = Id,
                StationId = _stationId,
                MaterialColor = $"#{MaterialColor.A:X2}{MaterialColor.R:X2}{MaterialColor.G:X2}{MaterialColor.B:X2}",
                Name = ModelName,
                Description = Desprition,
                CameraBackgroundColor =
                    $"#{CameraBackground.A:X2}{CameraBackground.R:X2}{CameraBackground.G:X2}{CameraBackground.B:X2}",
                CameraPosition = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}",
                CameraLookDirection = $"{Camera.LookDirection.X},{Camera.LookDirection.Y},{Camera.LookDirection.Z}",
                CameraUpDirection = $"{Camera.UpDirection.X},{Camera.UpDirection.Y},{Camera.UpDirection.Z}",
                FieldViewWidth = orthographicCamera?.Width ?? 500000,
                NearPlaneDistance = orthographicCamera?.NearPlaneDistance ?? 20000,
                FarPlaneDistance = orthographicCamera?.FarPlaneDistance ?? 100
            };
            var modelList = Model3DFiles.Select(s => new Model3DData()
            {
                Key = s.Key,
                Name = s.FileName,
                FilePath = s.FilePath,
                Type = s.BindedControlChannelIds.Count == 0 ? ModelType.Other : ModelType.Actuator,
                PositionStr = "0,0,0",
                RotationStr = "",
                RotateAngle = 90,
                ScaleStr = "1,1,1",
                DeviceId = null,
                Orientation = "0,-1,0"
            }).ToList();
            var channelAndModels = (from item in Model3DFiles
                    from controlChannelId in item.BindedControlChannelIds
                    select new ControlChannelAndModel3DInfo
                    {
                        ControlChannelId = controlChannelId,
                        ModelFileId = item.Key,
                        ModelId = modelBaseInfo.Id
                    })
                .ToList();
            var isSuccess = false;
            if (_isAdded)
            {
                var success =
                    await _model3DDataRepository.UpdateModel3DAsync(modelBaseInfo, modelList, channelAndModels);
                isSuccess = success; 
            }
            else
            {
                var addId = await _model3DDataRepository.AddModel3DAsync(modelBaseInfo, modelList);
                var addList = (from item in Model3DFiles
                        from controlChannelId in item.BindedControlChannelIds
                        select new ControlChannelAndModel3DInfo
                        {
                            ControlChannelId = controlChannelId,
                            ModelFileId = item.Key,
                            ModelId = addId
                        })
                    .ToList();
                var success = await _stationSiteAggregateRepository.AddControlChannelAndModelInfoAsync(addList);
                isSuccess = addId > 0 && success;
            }
            if (isSuccess)
            {
                _notificationService.Show("模型文件保存成功", $"保存成功{Model3DFiles.Count}个文件!");
            }
            else
            {
                _notificationService.Show("模型文件保存失败", $"保存失败!", NotificationType.Error);
            }

        }

        private async Task ExecuteLoadCommand()
        {
            if (_stationId == -1) throw new ArgumentNullException("StationId is null");
            var modelInfo = await _model3DDataRepository.GetModelAggregateByStationIdAsync(_stationId);
            Model3DFiles.Clear();
            GroupModel.Clear();
            if (modelInfo != null)
            {
                var bindedChannels =
                    await _stationSiteAggregateRepository.GetControlChannelAndModelInfoByModelIdAsync(modelInfo.BaseInfo
                        .Id);
                _isAdded = true;
                Id = modelInfo.BaseInfo.Id;
                ModelName = modelInfo.BaseInfo.Name;
                MaterialColor = (Color)ColorConverter.ConvertFromString(modelInfo.BaseInfo.MaterialColor);
                Desprition = modelInfo.BaseInfo.Description ?? "";
                CameraBackground = (Color)ColorConverter.ConvertFromString(modelInfo.BaseInfo.CameraBackgroundColor); 
                Camera = new HelixToolkit.Wpf.SharpDX.OrthographicCamera
                {
                    Position = modelInfo.BaseInfo.CameraPosition.ToVector<Point3D>(),
                    LookDirection = modelInfo.BaseInfo.CameraLookDirection.ToVector<Vector3D>(),
                    UpDirection = modelInfo.BaseInfo.CameraUpDirection.ToVector<Vector3D>(),
                    NearPlaneDistance = modelInfo.BaseInfo.NearPlaneDistance,    // 提高到100米
                    FarPlaneDistance = modelInfo.BaseInfo.FarPlaneDistance,    // 扩展到200公里
                    Width = modelInfo.BaseInfo.FieldViewWidth
                };
                foreach (var item in modelInfo.Model3DDataList)
                {
                    Model3DFiles.Add(new Model3DFileItemModel
                    {
                        FilePath = item.FilePath,
                        Id = item.Id,
                        Key = item.Key,
                        FileName = item.Name,
                        BindedControlChannelIds = bindedChannels
                            .Where(c => c.ModelFileId == item.Key)
                            .Select(s => s.ControlChannelId)
                            .ToList()
                    });
                }
                await LoadFileAsync(Model3DFiles.Select(s => s.FilePath).ToArray());
                UpdateMaterial(GroupModel.SceneNode, ColorConvertPhongMaterial(_materialColor));
            }
            else
            {
                _isAdded = false;
                ModelName = "";
                Desprition = "";
                Id = 0;
                Camera = new HelixToolkit.Wpf.SharpDX.OrthographicCamera
                {
                    Position = new Point3D(9055.2, 9739.8, 56394.1),
                    LookDirection = new Vector3D(3857.2, -4972.6, -63065.0),
                    UpDirection = new Vector3D(-0.0, 1.0, 0.1),
                    NearPlaneDistance = 100.0f,    // 提高到100米
                    FarPlaneDistance = 200000f,    // 扩展到200公里
                    Width = 54555.51994711041
                };
            }
            HaveData = Model3DFiles.Count > 0;
            BindingControlChannels.Clear();
            var controlChannels = await _stationSiteAggregateRepository.GetStationSiteControlChannels(_stationId);
            foreach (var controlChannel in controlChannels)
            {
                BindingControlChannels.Add(new BindingControlChannelItemModel
                {
                    ControlChannelName = controlChannel.ChannelName,
                    Id = controlChannel.Id,
                    IsSelected = false
                });
            }
        }

        private void ExecuteSelectMaterialCommand()
        {
            //var material = MaterialTypeEnumToMaterial.GetMaterialByEnum(selectMaterial);
            //CommonMathNode(GroupModel.SceneNode, _selectedModel3DFile.Key, UpdateMaterial, material);
        }

        private void UpdateMaterial(SceneNode sceneNode, Material? material)
        {
            foreach (var node in sceneNode.Traverse())
            {
                if (node is not MaterialGeometryNode m) continue;
                m.Material = material;
            }
        } 

        /// <summary>
        /// 删除匹配的节点
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="key"></param>
        private void RemoveMathNode(SceneNode rootNode, string key)
        {  
            CommonMathNode(rootNode, key, (node, material) =>
            {
                GroupModel.RemoveNode(node);
                node.Dispose();
            }, null);
        }

        /// <summary>
        /// 通用操作/针对匹配的节点
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="key"></param>
        /// <param name="execAction"></param>
        /// <param name="material"></param>
        private void CommonMathNode(SceneNode? rootNode, string key, Action<SceneNode, Material?> execAction, Material? material)
        {
            if (rootNode == null) return;
            var temp = rootNode.Tag as string;
            if (temp == key)
            {
                execAction?.Invoke(rootNode, material);
                return;
            }
            var childrenNodes = rootNode.Items.ToList();
            foreach (var child in childrenNodes)
            {
                CommonMathNode(child, key, execAction, material);
            }
        }

        private void ExecuteDeleteFileCommand(string key)
        {
            var removeObj = Model3DFiles.FirstOrDefault(c => c.Key == key);
            if (removeObj != null) Model3DFiles.Remove(removeObj);
            // 清理旧节点
            Task.Run(() =>
            {
                RemoveMathNode(GroupModel.SceneNode, key);
            });
            HaveData = Model3DFiles.Count > 0;
        }

        private async Task LoadFileAsync(string[] files)
        {
            try
            {
                var rootNode = await Task.Run(() =>
                {
                    var loader = new Importer();
                    loader.Configuration.AssimpPostProcessSteps = PostProcessSteps.Triangulate |
                                                                  PostProcessSteps.GenerateNormals |
                                                                  PostProcessSteps.FlipUVs;
                    var root = new GroupNode();
                    foreach (var path in files)
                    {
                        try
                        {
                            var file = Model3DFiles.FirstOrDefault(c => c.FilePath == path);
                            var scene = loader.Load(path);
                            if (scene?.Root != null)
                            {
                                scene.Root.Tag = file?.Key;
                                UpdateMaterial(scene.Root, EnumToMaterial.GetMaterialFromEnum(MaterialEnum.Original));
                                root.AddChildNode(scene.Root);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"导入文件 {path} 失败: {ex.Message}");
                        }
                    }

                    root.Attach(EffectsManager);
                    root.UpdateAllTransformMatrix();

                    return root;
                });
                if (rootNode != null)
                {
                    GroupModel.AddNode(rootNode);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"导入文件时发生错误: {ex.Message}");
            }
            finally
            {
                // IsLoading = false;
            }
        }

        private async Task ExecuteImportModelFileCommand()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择要导入的文件",
                Multiselect = true,
                Filter = $"{Importer.SupportedFormatsString}"
            };
            if (openFileDialog.ShowDialog() != true) return;
            foreach (var fileName in openFileDialog.FileNames)
            {
                Model3DFiles.Add(new Model3DFileItemModel
                {
                    Key = Guid.NewGuid().ToString("N"),
                    FileName = Path.GetFileNameWithoutExtension(fileName),
                    FilePath = fileName
                });
            }
            HaveData = Model3DFiles.Count > 0;
            await LoadFileAsync(openFileDialog.FileNames);
        }

        private void ExecuteAddBillboardTextCommand()
        {
            var textInfo = new TextInfoExt()
            {
                Text = $"DefaultName{CollectionDataLabels.TextInfo.Count + 1}",
                Scale = 1.0f,
                Background = SharpDX.Color.Black,
                Foreground = SharpDX.Color.White,
                Origin = new Vector3(1, 1, 1),
                Padding = new Vector4(5),
                Size = 14
            };
            BindedChannelModelBillboardTextInfos.Add(new BindedChannelModelBillboardTextInfo
            {
                Id = -1,
                BackgroundColor = SharpDX.Color.Black,
                FontColor = SharpDX.Color.White,
                BillboardName = $"DefaultName{CollectionDataLabels.TextInfo.Count + 1}",
                BindedControlChannelId = -1,
                BindedModelFileId = -1,
                BindedModelId = -1,
                FontSize = 14
            });
            CollectionDataLabels.TextInfo.Add(textInfo);
        }
        #endregion
    }
}
