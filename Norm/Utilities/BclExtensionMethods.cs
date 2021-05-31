namespace Norm.Utilities
{
    public static class BclExtensionMethods
    {
        public static int Modulo(this int number, int @base)
        {
            @base = @base < 0 ? -@base : @base;
            int remainder = number % @base;
            return remainder < 0 ? remainder + @base : remainder;
        }
    }
}
