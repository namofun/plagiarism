﻿using System;
using System.Linq;
using System.Security.Cryptography;

namespace Xylab.PlagiarismDetect.Backend
{
    public readonly struct SetGuid : IEquatable<SetGuid>
    {
        public const byte Flag = 0x95;

        private readonly Guid _guid;

        private SetGuid(Guid guid)
        {
            _guid = guid;
        }

        public static SetGuid New()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(guidBytes[6..10]);

            long timestamp = DateTime.UtcNow.Ticks / 10000L;
            Span<byte> timestampBytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(timestampBytes, timestamp);
            if (BitConverter.IsLittleEndian) timestampBytes.Reverse();
            timestampBytes[2..8].CopyTo(guidBytes[..6]);

            return UnsafeGet(guidBytes);
        }

        internal static SetGuid UnsafeGet(Span<byte> guidBytes)
        {
            // Assert that input.Length = 16 and first 10 bytes are valid
            byte checksum = EntityGuidHelper.CheckSum(guidBytes, Flag);
            guidBytes[10] = (byte)(guidBytes[5] ^ guidBytes[8]);
            guidBytes[11] = (byte)(guidBytes[2] ^ guidBytes[6]);
            guidBytes[12] = (byte)(guidBytes[1] ^ guidBytes[7]);
            guidBytes[13] = (byte)(guidBytes[4] ^ guidBytes[5]);
            guidBytes[14] = (byte)(guidBytes[0] ^ guidBytes[9]);
            guidBytes[15] = (byte)(guidBytes[3] ^ guidBytes[5]);
            guidBytes[9] = checksum;

            EntityGuidHelper.OperateIfLittleEndian(guidBytes);
            return new SetGuid(new Guid(guidBytes));
        }

        public static bool TryParse(ReadOnlySpan<char> input, out SetGuid setId)
        {
            if (!Guid.TryParse(input, out Guid guid))
            {
                setId = default;
                return false;
            }

            Span<byte> guidBytes = stackalloc byte[16];
            guid.TryWriteBytes(guidBytes);
            EntityGuidHelper.OperateIfLittleEndian(guidBytes);

            byte guidByte9 = EntityGuidHelper.CheckSum(guidBytes, Flag);
            if (guidBytes[10] != (byte)(guidBytes[5] ^ guidBytes[8])
                || guidBytes[11] != (byte)(guidBytes[2] ^ guidBytes[6])
                || guidBytes[12] != (byte)(guidBytes[1] ^ guidBytes[7])
                || guidBytes[13] != (byte)(guidBytes[4] ^ guidBytes[5])
                || guidBytes[14] != (byte)(guidBytes[0] ^ guidByte9)
                || guidBytes[15] != (byte)(guidBytes[3] ^ guidBytes[5]))
            {
                setId = default;
                return false;
            }

            setId = new SetGuid(guid);
            return true;
        }

        public static SetGuid Parse(ReadOnlySpan<char> input)
        {
            return TryParse(input, out SetGuid result)
                ? result
                : throw new FormatException("input is not in the correct format.");
        }

        public override string ToString()
        {
            return _guid.ToString();
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public bool Equals(SetGuid obj)
        {
            return this._guid == obj._guid;
        }

        public override bool Equals(object obj)
        {
            return obj is SetGuid other && Equals(other);
        }

        public static bool operator ==(SetGuid left, SetGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SetGuid left, SetGuid right)
        {
            return !left.Equals(right);
        }

        public bool TryWriteBytes(Span<byte> destination)
        {
            return _guid.TryWriteBytes(destination);
        }
    }

    public readonly struct SubmissionGuid : IEquatable<SubmissionGuid>
    {
        public const byte Flag = 0x37;

        private readonly Guid _guid;

        private SubmissionGuid(Guid guid)
        {
            _guid = guid;
        }

        public SetGuid GetSetId()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            _guid.TryWriteBytes(guidBytes);
            EntityGuidHelper.OperateIfLittleEndian(guidBytes);

            byte guidByte9 = EntityGuidHelper.CheckSum(guidBytes, Flag);
            guidBytes[9] = guidByte9;
            return SetGuid.UnsafeGet(guidBytes);
        }

        public int GetSubmissionId()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            _guid.TryWriteBytes(guidBytes);
            if (BitConverter.IsLittleEndian) guidBytes[12..16].Reverse();
            return BitConverter.ToInt32(guidBytes[12..16]) + EntityGuidHelper.Int24MinValue;
        }

        public static SubmissionGuid FromStructured(SetGuid guid, int sid)
        {
            EntityGuidHelper.ValidateInRange(nameof(sid), sid);

            Span<byte> guidBytes = stackalloc byte[16];
            guid.TryWriteBytes(guidBytes);

            guidBytes[9] = (byte)(guidBytes[9] ^ (SetGuid.Flag ^ Flag));
            BitConverter.TryWriteBytes(guidBytes[12..16], sid - EntityGuidHelper.Int24MinValue);
            if (BitConverter.IsLittleEndian) guidBytes[12..16].Reverse();
            guidBytes[10..13].Fill(0x00);

            return new SubmissionGuid(new Guid(guidBytes));
        }

        public static bool TryParse(ReadOnlySpan<char> input, out SubmissionGuid extId)
        {
            if (!Guid.TryParse(input, out Guid guid))
            {
                extId = default;
                return false;
            }
            else
            {
                extId = new SubmissionGuid(guid);
                return true;
            }
        }

        public static SubmissionGuid Parse(ReadOnlySpan<char> input)
        {
            return TryParse(input, out SubmissionGuid result)
                ? result
                : throw new FormatException("input is not in the correct format.");
        }

        public override string ToString()
        {
            return _guid.ToString();
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public bool Equals(SubmissionGuid obj)
        {
            return this._guid == obj._guid;
        }

        public override bool Equals(object obj)
        {
            return obj is SubmissionGuid other && Equals(other);
        }

        public static bool operator ==(SubmissionGuid left, SubmissionGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SubmissionGuid left, SubmissionGuid right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct ReportGuid : IEquatable<ReportGuid>
    {
        public const byte Flag = 0x6c;

        private readonly Guid _guid;

        private ReportGuid(Guid guid)
        {
            _guid = guid;
        }

        public SetGuid GetSetId()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            _guid.TryWriteBytes(guidBytes);
            EntityGuidHelper.OperateIfLittleEndian(guidBytes);

            byte guidByte9 = EntityGuidHelper.CheckSum(guidBytes, Flag);
            guidBytes[9] = guidByte9;
            return SetGuid.UnsafeGet(guidBytes);
        }

        public int GetIdOfA()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            _guid.TryWriteBytes(guidBytes);
            guidBytes[9] = 0;
            if (BitConverter.IsLittleEndian) guidBytes[9..13].Reverse();
            return BitConverter.ToInt32(guidBytes[9..13]) + EntityGuidHelper.Int24MinValue;
        }

        public int GetIdOfB()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            _guid.TryWriteBytes(guidBytes);
            guidBytes[12] = 0;
            if (BitConverter.IsLittleEndian) guidBytes[12..16].Reverse();
            return BitConverter.ToInt32(guidBytes[12..16]) + EntityGuidHelper.Int24MinValue;
        }

        public static ReportGuid FromStructured(SetGuid guid, int aid, int bid)
        {
            EntityGuidHelper.ValidateInRange(nameof(aid), aid);
            EntityGuidHelper.ValidateInRange(nameof(bid), bid);

            Span<byte> guidBytes = stackalloc byte[16];
            guid.TryWriteBytes(guidBytes);

            byte guidByte9 = (byte)(guidBytes[9] ^ (SetGuid.Flag ^ Flag));
            BitConverter.TryWriteBytes(guidBytes[12..16], bid - EntityGuidHelper.Int24MinValue);
            if (BitConverter.IsLittleEndian) guidBytes[12..16].Reverse();
            BitConverter.TryWriteBytes(guidBytes[9..13], aid - EntityGuidHelper.Int24MinValue);
            if (BitConverter.IsLittleEndian) guidBytes[9..13].Reverse();
            guidBytes[9] = guidByte9;

            return new ReportGuid(new Guid(guidBytes));
        }

        public static bool TryParse(ReadOnlySpan<char> input, out ReportGuid extId)
        {
            if (!Guid.TryParse(input, out Guid guid))
            {
                extId = default;
                return false;
            }
            else
            {
                extId = new ReportGuid(guid);
                return true;
            }
        }

        public static ReportGuid Parse(ReadOnlySpan<char> input)
        {
            return TryParse(input, out ReportGuid result)
                ? result
                : throw new FormatException("input is not in the correct format.");
        }

        public override string ToString()
        {
            return _guid.ToString();
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public bool Equals(ReportGuid obj)
        {
            return this._guid == obj._guid;
        }

        public override bool Equals(object obj)
        {
            return obj is ReportGuid other && Equals(other);
        }

        public static bool operator ==(ReportGuid left, ReportGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReportGuid left, ReportGuid right)
        {
            return !left.Equals(right);
        }
    }

    internal static class EntityGuidHelper
    {
        public const int Int24MinValue = -8388608;
        public const int Int24MaxValue = 8388607;

        public static byte CheckSum(ReadOnlySpan<byte> guidBytes, byte checksum)
        {
            for (int i = 0; i < 10; i++) checksum = (byte)(checksum ^ guidBytes[i]);
            return checksum;
        }

        public static void OperateIfLittleEndian(Span<byte> guidBytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                guidBytes[..4].Reverse();
                guidBytes[4..6].Reverse();
                guidBytes[6..8].Reverse();
            }
        }

        public static void ValidateInRange(string paramName, int int24)
        {
            if (!int24.CanBeInt24())
            {
                throw new ArgumentOutOfRangeException(paramName, "ID must be in range of signed 24bit integer.");
            }
        }

        public static bool CanBeInt24(this int int24)
        {
            return int24 > Int24MinValue && int24 <= Int24MaxValue;
        }
    }
}
