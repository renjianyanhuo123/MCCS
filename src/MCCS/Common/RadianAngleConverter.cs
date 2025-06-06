namespace MCCS.Common
{
    public static class RadianAngleConverter
    {
        public static double ToRadian(this double angle)
        {
            return angle * Math.PI / 180.0;
        }

        public static double ToAngle(this double angle)
        {
            return angle * 180.0 / Math.PI;
        }
    }
}
