using MCCS.Collecter.ControllerManagers;

namespace MCCS.Collecter.ControlChannelManagers
{
    public class ControlChannel
    { 
        private readonly IControllerManager _controllerManager;
        
        public ControlChannel(ControlChannelConfiguration configuration)
        {
            Configuration = configuration;
            ChannelId = configuration.ChannelId;
        }

        public ControlChannel(ControlChannelConfiguration configuration, IControllerManager controllerManager) : this(configuration)
        {
            _controllerManager = controllerManager;
        }

        public ControlChannelConfiguration Configuration { get; }

        public long ChannelId { get; } 
    }
}
