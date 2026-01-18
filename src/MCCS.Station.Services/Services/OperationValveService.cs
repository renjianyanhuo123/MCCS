using MCCS.Station.Abstractions.Dtos;
using MCCS.Station.Core.ControlChannelManagers;
using MCCS.Station.Services.IServices;

namespace MCCS.Station.Services.Services
{
    public class OperationValveService : IOperationValveService
    {
        private readonly IControlChannelManager _controlChannelManager;

        public OperationValveService(IControlChannelManager controlChannelManager)
        {
            _controlChannelManager = controlChannelManager;
        }

        public bool Execute(OperationValveCommandDto commandDto) 
        {
            var controlChannel = _controlChannelManager.GetControlChannel(commandDto.ControlChannelId);
            if (controlChannel == null) throw new InvalidOperationException($"Control channel with ID {commandDto.ControlChannelId} not found."); 
            return controlChannel.OperationValve(commandDto.OperationValve);
        }
    }
}
