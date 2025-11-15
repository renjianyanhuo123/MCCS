using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.SignalManagers;

namespace MCCS.Collecter.ControlChannelManagers
{
    public class ControlChannel
    { 
        private readonly IControllerManager _controllerManager;
        private readonly ISignalManager _signalManager;


        public ControlChannel(ControlChannelConfiguration configuration)
        {
            Configuration = configuration;
            ChannelId = configuration.ChannelId;
        }

        public ControlChannel(ControlChannelConfiguration configuration, IControllerManager controllerManager, ISignalManager signalManager) : this(configuration)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
        }

        public ControlChannelConfiguration Configuration { get; }

        public long ChannelId { get; } 
    }
}
