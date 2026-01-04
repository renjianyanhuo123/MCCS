using System.Text;

namespace MCCS.Station.Core.DllNative
{
    public class PopnetSettingModel
    {
        public int Maxchan { get; private set; }

        public double TransTimer { get; private set; }

        public double CtrlTimer { get; private set; }

        public int DataLogFreq { get; private set; }

        public int AdFrequency { get; private set; }

        public List<string> IPAddrs { get; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"""
                           maxchan={Maxchan}
                           TransTimer={TransTimer}
                           CtrlTimer={CtrlTimer}
                           DataLogFreq={DataLogFreq}
                           AdFrequency={AdFrequency}
                           """);
            for (var i = 0; i < IPAddrs.Count; i++)
            {
                sb.AppendLine($"IPAddr_{i}={IPAddrs[0]}");
            }
            return sb.ToString();
        }

        public static PopnetSettingModel Deserialize(string configString)
        {
            var config = new PopnetSettingModel();

            var lines = configString.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var equalIndex = line.IndexOf('=');
                if (equalIndex <= 0)
                    continue;

                var key = line[..equalIndex].Trim();
                var value = line.Substring(equalIndex + 1).Trim();

                switch (key.ToLower())
                {
                    case "maxchan":
                        config.Maxchan = int.Parse(value);
                        break;
                    case "transtimer":
                        config.TransTimer = double.Parse(value);
                        break;
                    case "ctrltimer":
                        config.CtrlTimer = double.Parse(value);
                        break;
                    case "datalogfreq":
                        config.DataLogFreq = int.Parse(value);
                        break;
                    case "adfrequency":
                        config.AdFrequency = int.Parse(value);
                        break;
                    default:
                        if (key.StartsWith("ipaddr_", StringComparison.OrdinalIgnoreCase))
                        {
                            config.IPAddrs.Add(value);
                        }
                        break;
                }
            }

            return config;
        }
    }
}
