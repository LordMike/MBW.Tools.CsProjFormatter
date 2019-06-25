﻿using System;
using System.Collections.Generic;
using EditorConfig.Core;
using MBW.Tools.CsProjFormatter.Library.Configuration;
using IndentStyle = EditorConfig.Core.IndentStyle;

namespace MBW.Tools.CsProjFormatter.Configuration.EditorConfig
{
    public static class EditorConfigExtensions
    {
        private static EditorConfigParser _parser = new EditorConfigParser();

        public static FormatterSettingsFactory AddEditorConfig(this FormatterSettingsFactory factory)
        {
            return factory.AddProvider(ProcessEditorConfig);
        }

        private static void ProcessEditorConfig(string file, FormatterSettings settings)
        {
            IEnumerable<FileConfiguration> result = _parser.Parse(file);

            foreach (FileConfiguration configuration in result)
            {
                if (configuration.IndentStyle.HasValue)
                {
                    settings.IndentStyle = configuration.IndentStyle == IndentStyle.Tab
                        ? Library.Configuration.IndentStyle.Tab
                        : Library.Configuration.IndentStyle.Space;
                    settings.IndentCount = configuration.IndentSize.NumberOfColumns ?? 1;
                }

                if (configuration.EndOfLine.HasValue)
                {
                    switch (configuration.EndOfLine.Value)
                    {
                        case EndOfLine.LF:
                            settings.NewlineCharacter = "\n";
                            break;
                        case EndOfLine.CR:
                            settings.NewlineCharacter = "\r";
                            break;
                        case EndOfLine.CRLF:
                            settings.NewlineCharacter = "\r\n";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (configuration.Charset.HasValue)
                {
                    switch (configuration.Charset.Value)
                    {
                        case Charset.Latin1:
                            settings.Encoding = "iso-8859-1";
                            break;
                        case Charset.UTF8:
                            settings.Encoding = "utf-8";
                            break;
                        case Charset.UTF8BOM:
                            settings.Encoding = "utf-8-bom";
                            break;
                        case Charset.UTF16BE:
                            settings.Encoding = "utf-16BE";
                            break;
                        case Charset.UTF16LE:
                            settings.Encoding = "utf-16";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}