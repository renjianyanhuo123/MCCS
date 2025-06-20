using MCCS.Models;
using System.Globalization;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class CommandStatusToEnableConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CommandExecuteStatusEnum commandExecuteStatus && parameter != null)
            {
                // 应用按钮对应状态启用
                if ((commandExecuteStatus == CommandExecuteStatusEnum.Stoping 
                    || commandExecuteStatus == CommandExecuteStatusEnum.NoExecute 
                    || commandExecuteStatus == CommandExecuteStatusEnum.ExecuttionCompleted) && parameter.ToString() == "Apply") 
                {
                    return true;
                }
                return commandExecuteStatus == CommandExecuteStatusEnum.Executing && parameter.ToString() == "Stop";
            }
            return false; // 默认禁用状态
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
