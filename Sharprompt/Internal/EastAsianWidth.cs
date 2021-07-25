using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sharprompt.Internal
{
    internal static class EastAsianWidth
    {
        public static int GetWidth(this char codePoint)
        {
            return IsFullWidth(codePoint) ? 2 : 1;
        }

        public static int GetWidth(this IEnumerable<char> value)
        {
            return value.Sum(GetWidth);
        }

        private static bool IsFullWidth(int codePoint)
        {
            var left = 0;
            var right = _ranges.Length - 1;

            while (left <= right)
            {
                var middle = left + (right - left) / 2;

                ref var range = ref _ranges[middle];

                if (codePoint < range.Start)
                {
                    right = middle - 1;

                    continue;
                }

                if (codePoint > range.Start + range.Count)
                {
                    left = middle + 1;

                    continue;
                }

                return !range.Ambiguous || IsEastAsianLanguage;
            }

            return false;
        }

        private static bool IsEastAsianLanguage => _languages.Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

        private static readonly string[] _languages = { "ja", "ko", "zh" };

        private readonly struct EastAsianWidthRange
        {
            public EastAsianWidthRange(int start, ushort count, bool ambiguous)
            {
                Start = start;
                Count = count;
                Ambiguous = ambiguous;
            }

            public int Start { get; }

            public ushort Count { get; }

            public bool Ambiguous { get; }
        }

        private static readonly EastAsianWidthRange[] _ranges =
        {
            new EastAsianWidthRange(161, 0, true),
            new EastAsianWidthRange(164, 0, true),
            new EastAsianWidthRange(167, 1, true),
            new EastAsianWidthRange(170, 0, true),
            new EastAsianWidthRange(173, 1, true),
            new EastAsianWidthRange(176, 4, true),
            new EastAsianWidthRange(182, 4, true),
            new EastAsianWidthRange(188, 3, true),
            new EastAsianWidthRange(198, 0, true),
            new EastAsianWidthRange(208, 0, true),
            new EastAsianWidthRange(215, 1, true),
            new EastAsianWidthRange(222, 3, true),
            new EastAsianWidthRange(230, 0, true),
            new EastAsianWidthRange(232, 2, true),
            new EastAsianWidthRange(236, 1, true),
            new EastAsianWidthRange(240, 0, true),
            new EastAsianWidthRange(242, 1, true),
            new EastAsianWidthRange(247, 3, true),
            new EastAsianWidthRange(252, 0, true),
            new EastAsianWidthRange(254, 0, true),
            new EastAsianWidthRange(257, 0, true),
            new EastAsianWidthRange(273, 0, true),
            new EastAsianWidthRange(275, 0, true),
            new EastAsianWidthRange(283, 0, true),
            new EastAsianWidthRange(294, 1, true),
            new EastAsianWidthRange(299, 0, true),
            new EastAsianWidthRange(305, 2, true),
            new EastAsianWidthRange(312, 0, true),
            new EastAsianWidthRange(319, 3, true),
            new EastAsianWidthRange(324, 0, true),
            new EastAsianWidthRange(328, 3, true),
            new EastAsianWidthRange(333, 0, true),
            new EastAsianWidthRange(338, 1, true),
            new EastAsianWidthRange(358, 1, true),
            new EastAsianWidthRange(363, 0, true),
            new EastAsianWidthRange(462, 0, true),
            new EastAsianWidthRange(464, 0, true),
            new EastAsianWidthRange(466, 0, true),
            new EastAsianWidthRange(468, 0, true),
            new EastAsianWidthRange(470, 0, true),
            new EastAsianWidthRange(472, 0, true),
            new EastAsianWidthRange(474, 0, true),
            new EastAsianWidthRange(476, 0, true),
            new EastAsianWidthRange(593, 0, true),
            new EastAsianWidthRange(609, 0, true),
            new EastAsianWidthRange(708, 0, true),
            new EastAsianWidthRange(711, 0, true),
            new EastAsianWidthRange(713, 2, true),
            new EastAsianWidthRange(717, 0, true),
            new EastAsianWidthRange(720, 0, true),
            new EastAsianWidthRange(728, 3, true),
            new EastAsianWidthRange(733, 0, true),
            new EastAsianWidthRange(735, 0, true),
            new EastAsianWidthRange(768, 111, true),
            new EastAsianWidthRange(913, 16, true),
            new EastAsianWidthRange(931, 6, true),
            new EastAsianWidthRange(945, 16, true),
            new EastAsianWidthRange(963, 6, true),
            new EastAsianWidthRange(1025, 0, true),
            new EastAsianWidthRange(1040, 63, true),
            new EastAsianWidthRange(1105, 0, true),
            new EastAsianWidthRange(4352, 95, false),
            new EastAsianWidthRange(8208, 0, true),
            new EastAsianWidthRange(8211, 3, true),
            new EastAsianWidthRange(8216, 1, true),
            new EastAsianWidthRange(8220, 1, true),
            new EastAsianWidthRange(8224, 2, true),
            new EastAsianWidthRange(8228, 3, true),
            new EastAsianWidthRange(8240, 0, true),
            new EastAsianWidthRange(8242, 1, true),
            new EastAsianWidthRange(8245, 0, true),
            new EastAsianWidthRange(8251, 0, true),
            new EastAsianWidthRange(8254, 0, true),
            new EastAsianWidthRange(8308, 0, true),
            new EastAsianWidthRange(8319, 0, true),
            new EastAsianWidthRange(8321, 3, true),
            new EastAsianWidthRange(8364, 0, true),
            new EastAsianWidthRange(8451, 0, true),
            new EastAsianWidthRange(8453, 0, true),
            new EastAsianWidthRange(8457, 0, true),
            new EastAsianWidthRange(8467, 0, true),
            new EastAsianWidthRange(8470, 0, true),
            new EastAsianWidthRange(8481, 1, true),
            new EastAsianWidthRange(8486, 0, true),
            new EastAsianWidthRange(8491, 0, true),
            new EastAsianWidthRange(8531, 1, true),
            new EastAsianWidthRange(8539, 3, true),
            new EastAsianWidthRange(8544, 11, true),
            new EastAsianWidthRange(8560, 9, true),
            new EastAsianWidthRange(8585, 0, true),
            new EastAsianWidthRange(8592, 9, true),
            new EastAsianWidthRange(8632, 1, true),
            new EastAsianWidthRange(8658, 0, true),
            new EastAsianWidthRange(8660, 0, true),
            new EastAsianWidthRange(8679, 0, true),
            new EastAsianWidthRange(8704, 0, true),
            new EastAsianWidthRange(8706, 1, true),
            new EastAsianWidthRange(8711, 1, true),
            new EastAsianWidthRange(8715, 0, true),
            new EastAsianWidthRange(8719, 0, true),
            new EastAsianWidthRange(8721, 0, true),
            new EastAsianWidthRange(8725, 0, true),
            new EastAsianWidthRange(8730, 0, true),
            new EastAsianWidthRange(8733, 3, true),
            new EastAsianWidthRange(8739, 0, true),
            new EastAsianWidthRange(8741, 0, true),
            new EastAsianWidthRange(8743, 5, true),
            new EastAsianWidthRange(8750, 0, true),
            new EastAsianWidthRange(8756, 3, true),
            new EastAsianWidthRange(8764, 1, true),
            new EastAsianWidthRange(8776, 0, true),
            new EastAsianWidthRange(8780, 0, true),
            new EastAsianWidthRange(8786, 0, true),
            new EastAsianWidthRange(8800, 1, true),
            new EastAsianWidthRange(8804, 3, true),
            new EastAsianWidthRange(8810, 1, true),
            new EastAsianWidthRange(8814, 1, true),
            new EastAsianWidthRange(8834, 1, true),
            new EastAsianWidthRange(8838, 1, true),
            new EastAsianWidthRange(8853, 0, true),
            new EastAsianWidthRange(8857, 0, true),
            new EastAsianWidthRange(8869, 0, true),
            new EastAsianWidthRange(8895, 0, true),
            new EastAsianWidthRange(8978, 0, true),
            new EastAsianWidthRange(8986, 1, false),
            new EastAsianWidthRange(9001, 1, false),
            new EastAsianWidthRange(9193, 3, false),
            new EastAsianWidthRange(9200, 0, false),
            new EastAsianWidthRange(9203, 0, false),
            new EastAsianWidthRange(9312, 137, true),
            new EastAsianWidthRange(9451, 96, true),
            new EastAsianWidthRange(9552, 35, true),
            new EastAsianWidthRange(9600, 15, true),
            new EastAsianWidthRange(9618, 3, true),
            new EastAsianWidthRange(9632, 1, true),
            new EastAsianWidthRange(9635, 6, true),
            new EastAsianWidthRange(9650, 1, true),
            new EastAsianWidthRange(9654, 1, true),
            new EastAsianWidthRange(9660, 1, true),
            new EastAsianWidthRange(9664, 1, true),
            new EastAsianWidthRange(9670, 2, true),
            new EastAsianWidthRange(9675, 0, true),
            new EastAsianWidthRange(9678, 3, true),
            new EastAsianWidthRange(9698, 3, true),
            new EastAsianWidthRange(9711, 0, true),
            new EastAsianWidthRange(9725, 1, false),
            new EastAsianWidthRange(9733, 1, true),
            new EastAsianWidthRange(9737, 0, true),
            new EastAsianWidthRange(9742, 1, true),
            new EastAsianWidthRange(9748, 1, false),
            new EastAsianWidthRange(9756, 0, true),
            new EastAsianWidthRange(9758, 0, true),
            new EastAsianWidthRange(9792, 0, true),
            new EastAsianWidthRange(9794, 0, true),
            new EastAsianWidthRange(9800, 11, false),
            new EastAsianWidthRange(9824, 1, true),
            new EastAsianWidthRange(9827, 2, true),
            new EastAsianWidthRange(9831, 3, true),
            new EastAsianWidthRange(9836, 1, true),
            new EastAsianWidthRange(9839, 0, true),
            new EastAsianWidthRange(9855, 0, false),
            new EastAsianWidthRange(9875, 0, false),
            new EastAsianWidthRange(9886, 1, true),
            new EastAsianWidthRange(9889, 0, false),
            new EastAsianWidthRange(9898, 1, false),
            new EastAsianWidthRange(9917, 2, false),
            new EastAsianWidthRange(9924, 29, false),
            new EastAsianWidthRange(9955, 0, true),
            new EastAsianWidthRange(9960, 23, true),
            new EastAsianWidthRange(9989, 0, false),
            new EastAsianWidthRange(9994, 1, false),
            new EastAsianWidthRange(10024, 0, false),
            new EastAsianWidthRange(10045, 0, true),
            new EastAsianWidthRange(10060, 0, false),
            new EastAsianWidthRange(10062, 0, false),
            new EastAsianWidthRange(10067, 2, false),
            new EastAsianWidthRange(10071, 0, false),
            new EastAsianWidthRange(10102, 9, true),
            new EastAsianWidthRange(10133, 2, false),
            new EastAsianWidthRange(10160, 0, false),
            new EastAsianWidthRange(10175, 0, false),
            new EastAsianWidthRange(11035, 1, false),
            new EastAsianWidthRange(11088, 0, false),
            new EastAsianWidthRange(11093, 4, false),
            new EastAsianWidthRange(11904, 25, false),
            new EastAsianWidthRange(11931, 88, false),
            new EastAsianWidthRange(12032, 213, false),
            new EastAsianWidthRange(12272, 11, false),
            new EastAsianWidthRange(12288, 62, false),
            new EastAsianWidthRange(12353, 85, false),
            new EastAsianWidthRange(12441, 102, false),
            new EastAsianWidthRange(12549, 42, false),
            new EastAsianWidthRange(12593, 93, false),
            new EastAsianWidthRange(12688, 83, false),
            new EastAsianWidthRange(12784, 46, false),
            new EastAsianWidthRange(12832, 7071, false),
            new EastAsianWidthRange(19968, 22156, false),
            new EastAsianWidthRange(42128, 54, false),
            new EastAsianWidthRange(43360, 28, false),
            new EastAsianWidthRange(44032, 11171, false),
            new EastAsianWidthRange(57344, 6911, true),
            new EastAsianWidthRange(65024, 25, true),
            new EastAsianWidthRange(65072, 34, false),
            new EastAsianWidthRange(65108, 18, false),
            new EastAsianWidthRange(65128, 3, false),
            new EastAsianWidthRange(65281, 95, false),
            new EastAsianWidthRange(65504, 6, false),
            new EastAsianWidthRange(65533, 0, true),
            new EastAsianWidthRange(94176, 4, false),
            new EastAsianWidthRange(94192, 1, false),
            new EastAsianWidthRange(94208, 6135, false),
            new EastAsianWidthRange(100352, 1237, false),
            new EastAsianWidthRange(101632, 8, false),
            new EastAsianWidthRange(110592, 286, false),
            new EastAsianWidthRange(110928, 2, false),
            new EastAsianWidthRange(110948, 3, false),
            new EastAsianWidthRange(110960, 395, false),
            new EastAsianWidthRange(126980, 0, false),
            new EastAsianWidthRange(127183, 0, false),
            new EastAsianWidthRange(127232, 10, true),
            new EastAsianWidthRange(127248, 29, true),
            new EastAsianWidthRange(127280, 57, true),
            new EastAsianWidthRange(127344, 60, true),
            new EastAsianWidthRange(127488, 2, false),
            new EastAsianWidthRange(127504, 43, false),
            new EastAsianWidthRange(127552, 8, false),
            new EastAsianWidthRange(127568, 1, false),
            new EastAsianWidthRange(127584, 5, false),
            new EastAsianWidthRange(127744, 32, false),
            new EastAsianWidthRange(127789, 8, false),
            new EastAsianWidthRange(127799, 69, false),
            new EastAsianWidthRange(127870, 21, false),
            new EastAsianWidthRange(127904, 42, false),
            new EastAsianWidthRange(127951, 4, false),
            new EastAsianWidthRange(127968, 16, false),
            new EastAsianWidthRange(127988, 0, false),
            new EastAsianWidthRange(127992, 70, false),
            new EastAsianWidthRange(128064, 0, false),
            new EastAsianWidthRange(128066, 186, false),
            new EastAsianWidthRange(128255, 62, false),
            new EastAsianWidthRange(128331, 3, false),
            new EastAsianWidthRange(128336, 23, false),
            new EastAsianWidthRange(128378, 0, false),
            new EastAsianWidthRange(128405, 1, false),
            new EastAsianWidthRange(128420, 0, false),
            new EastAsianWidthRange(128507, 84, false),
            new EastAsianWidthRange(128640, 69, false),
            new EastAsianWidthRange(128716, 0, false),
            new EastAsianWidthRange(128720, 2, false),
            new EastAsianWidthRange(128725, 2, false),
            new EastAsianWidthRange(128747, 1, false),
            new EastAsianWidthRange(128756, 8, false),
            new EastAsianWidthRange(128992, 11, false),
            new EastAsianWidthRange(129292, 46, false),
            new EastAsianWidthRange(129340, 9, false),
            new EastAsianWidthRange(129351, 49, false),
            new EastAsianWidthRange(129402, 81, false),
            new EastAsianWidthRange(129485, 50, false),
            new EastAsianWidthRange(129648, 4, false),
            new EastAsianWidthRange(129656, 2, false),
            new EastAsianWidthRange(129664, 6, false),
            new EastAsianWidthRange(129680, 24, false),
            new EastAsianWidthRange(129712, 6, false),
            new EastAsianWidthRange(129728, 2, false),
            new EastAsianWidthRange(129744, 6, false),
            new EastAsianWidthRange(131072, 65533, false),
            new EastAsianWidthRange(196608, 65533, false),
            new EastAsianWidthRange(917760, 239, true),
            new EastAsianWidthRange(983040, 65533, true),
            new EastAsianWidthRange(1048576, 65533, true)
        };
    }
}
