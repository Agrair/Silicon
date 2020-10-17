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

            if (TryUInt32(input, out uint color))
                result = getResult(color);
            else if (TryHex(input, out color))
                result = getResult(color);
            else if (TryRGB(input, out color))
                result = getResult(color);

            return Task.FromResult(result);

            static TypeReaderResult getResult(uint color) => TypeReaderResult.FromSuccess(new Color(color));
        }

        private bool TryUInt32(string input, out uint color, uint cap = 0xFFFFFF)
        {
            if (uint.TryParse(input, out color))
            {
                if (color > cap)
                    color = cap;
                return true;
            }
            return false;
        }

        private bool TryHex(string input, out uint color, uint cap = 0xFFFFFF)
        {
            color = 0;
            try
            {
                color = Convert.ToUInt32(input, 16);
                if (color > cap)
                    color = cap;
                return true;
            }
            catch { return false; }
        }

        private bool TryRGB(string input, out uint color)
        {
            color = 0;
            var split = input.Split(',', ':');
            if (split.Length != 3)
                return false;
            var trimmed = new string[3];
            for (int i = 0; i < split.Length; i++)
                trimmed[i] = split[i].Trim();
            var rgb = new uint[3];
            for (int i = 0; i < split.Length; i++)
            {
                if (TryUInt32(split[i], out uint value, 255))
                    rgb[i] = value;
                else if (TryHex(split[i], out value, 255))
                    rgb[i] = value;
                else
                    return false;
            }
            color = new Color(rgb[0] / 255, rgb[1] / 255, rgb[2] / 255).RawValue;
            return true;
        }
    }
}
