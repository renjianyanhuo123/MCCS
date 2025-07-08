using System.Windows;
using System.Windows.Controls;

namespace MCCS.UserControl.Pagination
{

    public class PaginationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextBlockTemplate { get; set; }

        public DataTemplate? ImageDataTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not SelectorUnitViewModel field) return null;
            return field.Type switch
            {
                UnitTypeEnum.Image => ImageDataTemplate,
                UnitTypeEnum.TextBlock => TextBlockTemplate,
                _ => null
            };
        }
    }
}
