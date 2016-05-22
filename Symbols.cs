﻿namespace Simulator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class Symbols
    {
        private Dictionary<ushort, string> labels;
        private Dictionary<ushort, string> constants;

        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> parsed;

        public Symbols(string path)
        {
            this.parsed = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            this.labels = new Dictionary<ushort, string>();
            this.constants = new Dictionary<ushort, string>();

            if (!string.IsNullOrWhiteSpace(path))
            {
                this.Parse(path);
                this.AssignSymbols();
            }
        }

        public Dictionary<ushort, string> Labels
        {
            get
            {
                return this.labels;
            }
        }

        public Dictionary<ushort, string> Constants
        {
            get
            {
                return this.constants;
            }
        }

        private void AssignSymbols()
        {
            var symbols = this.parsed["sym"];
            foreach (var symbol in symbols.Values)
            {
                var name = symbol["name"].Trim(new char[] { '"' });
                var value = symbol["val"];
                var parsed = ushort.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                switch (symbol["type"])
                {
                    case "lab":
                        this.labels[parsed] = name;
                        break;

                    case "equ":
                        this.constants[parsed] = name;
                        break;
                }
            }
        }

        private void Parse(string path)
        {
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineElements = line.Split();
                    if (lineElements.Length == 2)
                    {
                        var type = lineElements[0];
                        var dataElements = lineElements[1].Split(new char[] { ',' });
                        var data = new Dictionary<string, string>();
                        foreach (var dataElement in dataElements)
                        {
                            var definition = dataElement.Split(new char[] { '=' });
                            if (definition.Length == 2)
                            {
                                data[definition[0]] = definition[1];
                            }
                        }

                        if (data.ContainsKey("id"))
                        {
                            if (!this.parsed.ContainsKey(type))
                            {
                                this.parsed[type] = new Dictionary<string, Dictionary<string, string>>();
                            }

                            var id = data["id"];
                            data.Remove("id");
                            this.parsed[type][id] = data;
                        }
                    }
                }
            }
        }
    }
}
