using System.Collections.Concurrent;
using MCCS.Collecter.ControllerManagers;
using MCCS.Infrastructure.TestModels.Commands;
using MCCS.Infrastructure.TestModels.ControlParams;

namespace MCCS.Collecter.ControlChannelManagers
{
    public sealed class ControlChannelManager : IControlChannelManager
    {
        private readonly ConcurrentDictionary<long, ControlChannel> _channelDics = [];
        private readonly IControllerManager _controllerManager;

        public ControlChannelManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        }

        public void Initialization(IEnumerable<ControlChannelConfiguration> configurations)
        {
            foreach (var configuration in configurations)
            {
                _channelDics.TryAdd(configuration.ChannelId, new ControlChannel(configuration, _controllerManager));
            }
        }

        public bool AddControlChannel(ControlChannelConfiguration configuration)
        {
            return _channelDics.TryAdd(configuration.ChannelId, new ControlChannel(configuration));
        }

        public bool RemoveControlChannel(long channelId)
        {
            return _channelDics.Remove(channelId, out var tempChannel);
        }

        public DeviceCommandContext DynamicControl(long channelId, DynamicControlParams dynamicControlParam)
        {

        }

        public DeviceCommandContext ManualControl(long channelId, float speed)
        {
            throw new NotImplementedException();
        }

        public DeviceCommandContext StaticControl(long channelId, StaticControlParams controlParam)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        } 
    }
}
