using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Silicon.Commands.Commons
{
    public class ColorReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            TypeReaderResult result = TypeReaderResult.FromError(CommandError.ParseFailed,
                $"`{input}` is not a valid color.");

            if (TryUInt32(input, out uint color)) result = getResult(color);
            else if (TryHex(input, out color)) result = getResult(color);
            else if (TryRGB(input, out color)) result = getResult(color);

            return Task.FromResult(result);

            static TypeReaderResult getResult(uint color) => TypeReaderResult.FromSuccess(new Color(color));
        }

        static uint CapResult(uint v)
        {
            if (v > 0xFFFFFF) return 0xFFFFFF;
            return v;
        }

        private bool TryUInt32(string input, out uint color)
        {
            color = 0;
            if (uint.TryParse(input, out uint value))
            {
                if (value > 0xFFFFFF) value = 0xFFFFFF;
                color = CapResult(value);
                return true;
            }
            return false;
        }

        private bool TryHex(string input, out uint color)
        {
            color = 0;
            try
            {
                color = CapResult(Convert.ToUInt32(input, 16));
                return true;
            }
            catch { return false; }
        }

        private bool TryRGB(string input, out uint color)
        {
            color = 0;
            var split = input.Split(',', ':');
            if (split.Length != 3) return false;
            var trimmed = new string[3];
            for (int i = 0; i < split.Length; i++) trimmed[i] = split[i].Trim();
            var rgb = new uint[3];
            for (int i = 0; i < split.Length; i++)
            {
                if (TryUInt32(split[i], out uint value)) rgb[i] = value;
                else if (TryHex(split[i], out value)) rgb[i] = value;
                else return false;
            }
            color = new Color(rgb[0], rgb[1], rgb[2]).RawValue;
            return true;
        }
    }
}
