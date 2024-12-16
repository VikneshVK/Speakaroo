// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("DAaOk+PZsmNa2QrAuwMpx9EOJH/G3iGfdeeva4Kw+MgKRn/rSuwFMBaS/4YCBzzN6JRR0DGQF9R1kPHlV6tdL5xR8k/g73gZqhlkX4sboCOoGpm6qJWekbIe0B5vlZmZmZ2Ym6leVICimcB95jMLIFQenduktW8Fz6z1NmOduodvduZcIvKPwqQ5m/bku8Zvq8XCtamNCA1AmIE6ZgsVHjkuh3h5Xv8bV/PTnqtvpeKmjXRgPcpam8Md1L/2tYGewRwmQLeuVIvHXVCvKYa6uOHgfM2GvU3GV53ETsQa4TyAdts17AQmD+5rzXt11pEOGpmXmKgamZKaGpmZmAWUCqpe3ytUOUnv4w2gyOqVHxbRk+Te55xwFjW66o6uE3KVT5qbmZiZ");
        private static int[] order = new int[] { 10,4,9,9,10,8,13,12,11,12,11,12,12,13,14 };
        private static int key = 152;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
