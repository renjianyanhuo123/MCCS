namespace MCCS.Infrastructure.Helper
{
    public static class BitHelper
    {
        /// <summary>
        /// 将整数的指定位设置为对应值
        /// </summary>
        /// <param name="value">传入的值</param>
        /// <param name="bitPosition">bit位置</param>
        /// <param name="bitContent">0/1</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int SetBit(int value, int bitPosition, int bitContent)
        {
            if (bitPosition is < 0 or >= 31)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bitPosition),
                    "位位置必须在 0-30 之间");
            }

            if (bitContent == 1) return value | (1 << bitPosition);
            return value & ~(1 << bitPosition); 
        }

        /// <summary>
        /// 检查指定位是否为1
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool TestBit(int value, int bitPosition)
        {
            if (bitPosition is < 0 or >= 31)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bitPosition),
                    "位位置必须在 0-30 之间");
            }

            return (value & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// 切换指定位的状态（0变1，1变0）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int ToggleBit(int value, int bitPosition)
        {
            if (bitPosition is < 0 or >= 31)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bitPosition),
                    "位位置必须在 0-30 之间");
            }

            return value ^ (1 << bitPosition);
        }
    }
}
