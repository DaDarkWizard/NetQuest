using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.UI
{
    public class InputParser
    {
        private readonly string _input;
        public string[] ParsedInput { get; private set; }

        public InputParser(string? input)
        {
            _input = input?.Trim() ?? "";

            List<string> args = new();

            int parsed = 0;

            while (parsed < _input.Length)
            {
                StringBuilder builder = new();

                char start = _input[parsed];
                if(start == '"')
                {
                    parsed++;
                    while(parsed < _input.Length)
                    {
                        if(_input[parsed] == '"')
                        {
                            if(parsed + 1 < _input.Length && _input[parsed+1] == '"')
                            {
                                parsed++;
                                builder.Append('"');
                            }
                            else
                            {
                                parsed++;
                                break;
                            }
                        }
                        else
                        {
                            builder.Append(_input[parsed]);
                        }
                        parsed++;
                    }
                }
                else
                {
                    while (parsed < _input.Length && !char.IsWhiteSpace(_input[parsed]))
                    {
                        builder.Append(_input[parsed++]);
                    }
                }
                while (parsed < _input.Length && char.IsWhiteSpace(_input[parsed]))
                {
                    parsed++;
                }
                args.Add(builder.ToString());
            }

            ParsedInput = args.ToArray();
        }

        public string this[int i]
        { 
            get
            {
                return ParsedInput[i];
            }
        }

        public int Length { get => ParsedInput.Length; }

        public static bool operator ==(InputParser a, string b)
        {
            return a._input == b;
        }

        public static bool operator !=(InputParser a, string b)
        {
            return a._input != b;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            if (obj is InputParser parser)
            {
                return this._input == parser._input;
            }

            if (obj is string input)
            {
                return this._input == input;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _input.GetHashCode();
        }
    }
}
