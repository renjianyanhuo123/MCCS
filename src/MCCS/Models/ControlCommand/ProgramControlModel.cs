using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Models.ControlCommand
{
    public class ProgramControlModel
    {
        public required string ChannelId { get; set; }
        /// <summary>
        /// 程序控制文件路径
        /// </summary>
        public string? FilePath { get; set; }
    }
}
