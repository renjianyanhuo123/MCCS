using System.Collections.ObjectModel;
using System.Reflection;

namespace MCCS.Models.ControlCommand;

public sealed class CommandParamter
{
    public CommandParamter(string key, string value) 
    {
        Key = key;
        Value = value;
    }
    public string Key { get; set; }

    public string Value { get; set; }
}

public sealed class ControlProcessExpander : BindableBase
{ 
    private double _progressRate = 0.0;
    private object _paramter;

    public ControlProcessExpander(object param) 
    {
        _paramter = param;
        ControlCommandParams = GetKeyValuePairs(param);
    }

    /// <summary>
    /// 当前指令ID
    /// </summary>
    public string? CommandId {get; set; }

    /// <summary>
    /// 当前指令名称
    /// </summary>
    public string CommandName  { get; set;} = string.Empty;

    /// <summary>
    /// 当前指令执行进度值
    /// </summary>
    public double ProgressRate  
    {  
        get => _progressRate;
        set => SetProperty(ref _progressRate, value); 
    }

    /// <summary>
    /// 控制类型
    /// </summary>
    public ControlTypeEnum ControlType {get; set; }

    /// <summary>
    /// 控制模式
    /// </summary>
    public ControlMode ControlMode { get; set;}

    /// <summary>
    /// 参数列表
    /// </summary>
    public ObservableCollection<CommandParamter> ControlCommandParams { get ; private set ; }

    public void SetControlChannels(List<string> channelStrs) 
    {
        var str = string.Join(";", channelStrs);
        ControlCommandParams.Add(new CommandParamter("通道列表", str));
    }

    private ObservableCollection<CommandParamter> GetKeyValuePairs(object obj) 
    {
        var type = obj.GetType();
        var properties = type.GetProperties();
        var res = new ObservableCollection<CommandParamter>();
        foreach (var prop in properties)
        {
            var name = prop.Name;
            var value = prop.GetValue(obj) ?? string.Empty;
            res.Add(new CommandParamter(name, value.ToString() ?? string.Empty));
        }
        return res;
    }

    public Dictionary<string, object> GetParamDic() 
    {
        var type = _paramter.GetType();
        var properties = type.GetProperties();
        var res = new Dictionary<string, object>();
        foreach (var prop in properties)
        {
            var name = prop.Name;
            var value = prop.GetValue(_paramter) ?? string.Empty;
            res.Add(name, value);
        }
        return res;
    }
}
