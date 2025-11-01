namespace Archivarius.Internals
{
    public static class BitMath
    {
        /// <summary>
        /// Позиция старшего бита в числе
        /// </summary>
        public static int HiBit(uint n)
        {
            int r = 0;
            if (n > 0xFFFFU)
            {
                n >>= 16;
                r |= 16;
            }
            if (n > 0xFFU)
            {
                n >>= 8;
                r |= 8;
            }
            if (n > 0xFU)
            {
                n >>= 4;
                r |= 4;
            }
            if (n > 3)
            {
                n >>= 2;
                r |= 2;
            }
            if (n > 1)
            {
                r |= 1;
            }

            return r;
        }

        /// <summary>
        /// Минимальная степень двойки большая или равная заданному числу
        /// </summary>
        public static int NextPow2(uint n)
        {
            int res = 1 << HiBit(n);
            if (res < n)
            {
                res *= 2;
            }
            return res;
        }
    }
}
