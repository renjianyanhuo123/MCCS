using HelixToolkit.Wpf;
using MCCS.ViewModels.Others;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;

namespace MCCS.Behaviors
{
    public class ModelInteractionBehavior : Behavior<HelixViewport3D>
    {
        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<Model3DViewModel>),
                typeof(ModelInteractionBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand),
                typeof(ModelInteractionBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty HoverCommandProperty =
            DependencyProperty.Register(nameof(HoverCommand), typeof(ICommand),
                typeof(ModelInteractionBehavior), new PropertyMetadata(null));

        public ObservableCollection<Model3DViewModel> Models
        {
            get => (ObservableCollection<Model3DViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        public ICommand HoverCommand
        {
            get => (ICommand)GetValue(HoverCommandProperty);
            set => SetValue(HoverCommandProperty, value);
        }

        private Model3DViewModel _lastHoveredModel;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseDown += OnMouseDown;
                AssociatedObject.MouseMove += OnMouseMove;
                AssociatedObject.MouseLeave += OnMouseLeave;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseDown -= OnMouseDown;
                AssociatedObject.MouseMove -= OnMouseMove;
                AssociatedObject.MouseLeave -= OnMouseLeave;
            }
            base.OnDetaching();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var hitModel = GetHitModel(e.GetPosition(AssociatedObject));
                if (hitModel != null && ClickCommand?.CanExecute(hitModel) == true)
                {
                    ClickCommand.Execute(hitModel);
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var hitModel = GetHitModel(e.GetPosition(AssociatedObject));

            if (hitModel != _lastHoveredModel)
            {
                // 如果之前有悬停的模型，通知离开
                if (HoverCommand.CanExecute(null))
                {
                    HoverCommand.Execute(null);
                }

                // 如果现在有新的悬停模型，通知进入
                if (HoverCommand.CanExecute(hitModel))
                {
                    HoverCommand.Execute(hitModel);
                }

                _lastHoveredModel = hitModel;
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (HoverCommand == null && HoverCommand.CanExecute(null) != true) return;
            HoverCommand.Execute(null);
            _lastHoveredModel = null;
        }

        private Model3DViewModel GetHitModel(Point mousePosition)
        {
            if (AssociatedObject == null)
                return null; 
            var hitResult = VisualTreeHelper.HitTest(AssociatedObject.Viewport, mousePosition);
            if (hitResult is RayMeshGeometry3DHitTestResult { ModelHit: GeometryModel3D model })
            {
                // 在Models集合中查找对应的ViewModel
                return Models.FirstOrDefault(modelVm => modelVm.Model == model);
            }

            return null;
        }
    }
}
