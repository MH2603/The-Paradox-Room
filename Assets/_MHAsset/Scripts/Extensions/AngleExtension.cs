namespace MH

{
    public static class AngleExtension
    {
        public static float ToRadians(this float angle)
        {
            return (float)(angle * System.Math.PI / 180);
        }
    }
}