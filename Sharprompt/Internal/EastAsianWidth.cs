﻿namespace Sharprompt.Internal;

internal static class EastAsianWidth
{
    public static int GetWidth(this string value)
    {
        var width = 0;

        for (var i = 0; i < value.Length; i++)
        {
            uint codePoint;

            if (char.IsHighSurrogate(value[i]) && (i + 1 < value.Length && char.IsLowSurrogate(value[i + 1])))
            {
                codePoint = (uint)(0x10000 + ((value[i] - 0xD800) * 0x0400) + (value[i + 1] - 0xDC00));

                i++;
            }
            else
            {
                codePoint = value[i];
            }

            width += GetWidth(codePoint);
        }

        return width;
    }

    private static int GetWidth(uint codePoint) => IsFullWidth(codePoint) ? 2 : 1;

    private static bool IsFullWidth(uint codePoint)
    {
        var left = 0;
        var right = s_eastAsianWidthRanges.Length - 1;

        while (left <= right)
        {
            var center = left + (right - left) / 2;

            ref var range = ref s_eastAsianWidthRanges[center];

            if (codePoint < range.Start)
            {
                right = center - 1;

                continue;
            }

            if (codePoint > range.Start + range.Count)
            {
                left = center + 1;

                continue;
            }

            return !range.Ambiguous;
        }

        return false;
    }

    private static readonly EastAsianWidthRange[] s_eastAsianWidthRanges =
    {
        new(161, 0, true),
        new(164, 0, true),
        new(167, 1, true),
        new(170, 0, true),
        new(173, 1, true),
        new(176, 4, true),
        new(182, 4, true),
        new(188, 3, true),
        new(198, 0, true),
        new(208, 0, true),
        new(215, 1, true),
        new(222, 3, true),
        new(230, 0, true),
        new(232, 2, true),
        new(236, 1, true),
        new(240, 0, true),
        new(242, 1, true),
        new(247, 3, true),
        new(252, 0, true),
        new(254, 0, true),
        new(257, 0, true),
        new(273, 0, true),
        new(275, 0, true),
        new(283, 0, true),
        new(294, 1, true),
        new(299, 0, true),
        new(305, 2, true),
        new(312, 0, true),
        new(319, 3, true),
        new(324, 0, true),
        new(328, 3, true),
        new(333, 0, true),
        new(338, 1, true),
        new(358, 1, true),
        new(363, 0, true),
        new(462, 0, true),
        new(464, 0, true),
        new(466, 0, true),
        new(468, 0, true),
        new(470, 0, true),
        new(472, 0, true),
        new(474, 0, true),
        new(476, 0, true),
        new(593, 0, true),
        new(609, 0, true),
        new(708, 0, true),
        new(711, 0, true),
        new(713, 2, true),
        new(717, 0, true),
        new(720, 0, true),
        new(728, 3, true),
        new(733, 0, true),
        new(735, 0, true),
        new(768, 111, true),
        new(913, 16, true),
        new(931, 6, true),
        new(945, 16, true),
        new(963, 6, true),
        new(1025, 0, true),
        new(1040, 63, true),
        new(1105, 0, true),
        new(4352, 95, false),
        new(8208, 0, true),
        new(8211, 3, true),
        new(8216, 1, true),
        new(8220, 1, true),
        new(8224, 2, true),
        new(8228, 3, true),
        new(8240, 0, true),
        new(8242, 1, true),
        new(8245, 0, true),
        new(8251, 0, true),
        new(8254, 0, true),
        new(8308, 0, true),
        new(8319, 0, true),
        new(8321, 3, true),
        new(8364, 0, true),
        new(8451, 0, true),
        new(8453, 0, true),
        new(8457, 0, true),
        new(8467, 0, true),
        new(8470, 0, true),
        new(8481, 1, true),
        new(8486, 0, true),
        new(8491, 0, true),
        new(8531, 1, true),
        new(8539, 3, true),
        new(8544, 11, true),
        new(8560, 9, true),
        new(8585, 0, true),
        new(8592, 9, true),
        new(8632, 1, true),
        new(8658, 0, true),
        new(8660, 0, true),
        new(8679, 0, true),
        new(8704, 0, true),
        new(8706, 1, true),
        new(8711, 1, true),
        new(8715, 0, true),
        new(8719, 0, true),
        new(8721, 0, true),
        new(8725, 0, true),
        new(8730, 0, true),
        new(8733, 3, true),
        new(8739, 0, true),
        new(8741, 0, true),
        new(8743, 5, true),
        new(8750, 0, true),
        new(8756, 3, true),
        new(8764, 1, true),
        new(8776, 0, true),
        new(8780, 0, true),
        new(8786, 0, true),
        new(8800, 1, true),
        new(8804, 3, true),
        new(8810, 1, true),
        new(8814, 1, true),
        new(8834, 1, true),
        new(8838, 1, true),
        new(8853, 0, true),
        new(8857, 0, true),
        new(8869, 0, true),
        new(8895, 0, true),
        new(8978, 0, true),
        new(8986, 1, false),
        new(9001, 1, false),
        new(9193, 3, false),
        new(9200, 0, false),
        new(9203, 0, false),
        new(9312, 137, true),
        new(9451, 96, true),
        new(9552, 35, true),
        new(9600, 15, true),
        new(9618, 3, true),
        new(9632, 1, true),
        new(9635, 6, true),
        new(9650, 1, true),
        new(9654, 1, true),
        new(9660, 1, true),
        new(9664, 1, true),
        new(9670, 2, true),
        new(9675, 0, true),
        new(9678, 3, true),
        new(9698, 3, true),
        new(9711, 0, true),
        new(9725, 1, false),
        new(9733, 1, true),
        new(9737, 0, true),
        new(9742, 1, true),
        new(9748, 1, false),
        new(9756, 0, true),
        new(9758, 0, true),
        new(9792, 0, true),
        new(9794, 0, true),
        new(9800, 11, false),
        new(9824, 1, true),
        new(9827, 2, true),
        new(9831, 3, true),
        new(9836, 1, true),
        new(9839, 0, true),
        new(9855, 0, false),
        new(9875, 0, false),
        new(9886, 1, true),
        new(9889, 0, false),
        new(9898, 1, false),
        new(9917, 1, false),
        new(9919, 0, true),
        new(9924, 1, false),
        new(9926, 7, true),
        new(9934, 0, false),
        new(9935, 4, true),
        new(9940, 0, false),
        new(9941, 12, true),
        new(9955, 0, true),
        new(9960, 1, true),
        new(9962, 0, false),
        new(9963, 6, true),
        new(9970, 1, false),
        new(9972, 0, true),
        new(9973, 0, false),
        new(9974, 3, true),
        new(9978, 0, false),
        new(9979, 1, true),
        new(9981, 0, false),
        new(9982, 1, true),
        new(9989, 0, false),
        new(9994, 1, false),
        new(10024, 0, false),
        new(10045, 0, true),
        new(10060, 0, false),
        new(10062, 0, false),
        new(10067, 2, false),
        new(10071, 0, false),
        new(10102, 9, true),
        new(10133, 2, false),
        new(10160, 0, false),
        new(10175, 0, false),
        new(11035, 1, false),
        new(11088, 0, false),
        new(11093, 0, false),
        new(11094, 3, true),
        new(11904, 25, false),
        new(11931, 88, false),
        new(12032, 213, false),
        new(12272, 11, false),
        new(12288, 0, false),
        new(12289, 61, false),
        new(12353, 85, false),
        new(12441, 102, false),
        new(12549, 42, false),
        new(12593, 93, false),
        new(12688, 83, false),
        new(12784, 46, false),
        new(12832, 39, false),
        new(12872, 7, true),
        new(12880, 7023, false),
        new(19968, 22156, false),
        new(42128, 54, false),
        new(43360, 28, false),
        new(44032, 11171, false),
        new(57344, 6399, true),
        new(63744, 511, false),
        new(65024, 15, true),
        new(65040, 9, false),
        new(65072, 34, false),
        new(65108, 18, false),
        new(65128, 3, false),
        new(65281, 95, false),
        new(65504, 6, false),
        new(65533, 0, true),
        new(94176, 4, false),
        new(94192, 1, false),
        new(94208, 6135, false),
        new(100352, 1237, false),
        new(101632, 8, false),
        new(110576, 3, false),
        new(110581, 6, false),
        new(110589, 1, false),
        new(110592, 290, false),
        new(110928, 2, false),
        new(110948, 3, false),
        new(110960, 395, false),
        new(126980, 0, false),
        new(127183, 0, false),
        new(127232, 10, true),
        new(127248, 29, true),
        new(127280, 57, true),
        new(127344, 29, true),
        new(127374, 0, false),
        new(127375, 1, true),
        new(127377, 9, false),
        new(127387, 17, true),
        new(127488, 2, false),
        new(127504, 43, false),
        new(127552, 8, false),
        new(127568, 1, false),
        new(127584, 5, false),
        new(127744, 32, false),
        new(127789, 8, false),
        new(127799, 69, false),
        new(127870, 21, false),
        new(127904, 42, false),
        new(127951, 4, false),
        new(127968, 16, false),
        new(127988, 0, false),
        new(127992, 70, false),
        new(128064, 0, false),
        new(128066, 186, false),
        new(128255, 62, false),
        new(128331, 3, false),
        new(128336, 23, false),
        new(128378, 0, false),
        new(128405, 1, false),
        new(128420, 0, false),
        new(128507, 84, false),
        new(128640, 69, false),
        new(128716, 0, false),
        new(128720, 2, false),
        new(128725, 2, false),
        new(128733, 2, false),
        new(128747, 1, false),
        new(128756, 8, false),
        new(128992, 11, false),
        new(129008, 0, false),
        new(129292, 46, false),
        new(129340, 9, false),
        new(129351, 184, false),
        new(129648, 4, false),
        new(129656, 4, false),
        new(129664, 6, false),
        new(129680, 28, false),
        new(129712, 10, false),
        new(129728, 5, false),
        new(129744, 9, false),
        new(129760, 7, false),
        new(129776, 6, false),
        new(131072, 65533, false),
        new(196608, 65533, false),
        new(917760, 239, true),
        new(983040, 65533, true),
        new(1048576, 65533, true)
    };

    private readonly struct EastAsianWidthRange
    {
        public EastAsianWidthRange(uint start, ushort count, bool ambiguous)
        {
            Start = start;
            Count = count;
            Ambiguous = ambiguous;
        }

        public uint Start { get; }
        public ushort Count { get; }
        public bool Ambiguous { get; }
    }
}
