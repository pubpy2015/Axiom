﻿/* ----------------------------------------------------------------------
Axiom UI
Copyright (C) 2017-2020 Matt McManis
https://github.com/MattMcManis/Axiom
https://axiomui.github.io
mattmcmanis@outlook.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see <http://www.gnu.org/licenses/>. 
---------------------------------------------------------------------- */

/* ----------------------------------
 METHODS

 * Generate FFmpeg Args
---------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Axiom
{
    public class Batch
    {
        /// <summary>
        /// FFmpeg Batch - Generate Args
        /// </summary>
        public static void Generate_FFmpegArgs()
        {
            if (VM.MainView.Batch_IsChecked == true)
            {
                // Log Console Message /////////
                Log.WriteAction = () =>
                {
                    Log.logParagraph.Inlines.Add(new LineBreak());
                    Log.logParagraph.Inlines.Add(new LineBreak());
                    Log.logParagraph.Inlines.Add(new Bold(new Run("Batch: ")) { Foreground = Log.ConsoleDefault });
                    Log.logParagraph.Inlines.Add(new Run(Convert.ToString(VM.MainView.Batch_IsChecked)) { Foreground = Log.ConsoleDefault });
                    Log.logParagraph.Inlines.Add(new LineBreak());
                    Log.logParagraph.Inlines.Add(new LineBreak());
                    Log.logParagraph.Inlines.Add(new Bold(new Run("Generating Batch Script...")) { Foreground = Log.ConsoleTitle });
                    //Log.logParagraph.Inlines.Add(new LineBreak());
                    //Log.logParagraph.Inlines.Add(new LineBreak());
                    //Log.logParagraph.Inlines.Add(new Bold(new Run("Running Batch Convert...")) { Foreground = Log.ConsoleAction });
                    Log.logParagraph.Inlines.Add(new LineBreak());
                    Log.logParagraph.Inlines.Add(new Run(FFmpeg.ffmpegArgs) { Foreground = Log.ConsoleDefault });
                };
                Log.LogActions.Add(Log.WriteAction);


                List<string> ffmpegBatchArgsList = new List<string>();

                switch (VM.ConfigureView.Shell_SelectedItem)
                {
                    // -------------------------
                    // CMD
                    // -------------------------
                    case "CMD":
                        // -------------------------
                        // Batch Arguments Full
                        // -------------------------
                        ffmpegBatchArgsList = new List<string>()
                        {
                            "cd /d",
                            "\"" + MainWindow.BatchInputDirectory() + "\"",
                            //MainWindow.ShellStringFormatter(MainWindow.BatchInputDirectory()),

                            "\r\n\r\n" + "&& for %f in",
                            "(*" + MainWindow.inputExt + ")",
                            "do (echo)",

                            // Video
                            "\r\n\r\n" +
                            Video.BatchVideoQualityAuto(VM.MainView.Batch_IsChecked,
                                                        VM.VideoView.Video_Codec_SelectedItem,
                                                        VM.VideoView.Video_Quality_SelectedItem
                                                        ),

                            // Audio
                            "\r\n\r\n" +
                            Audio.BatchAudioQualityAuto(VM.MainView.Batch_IsChecked,
                                                        VM.AudioView.Audio_Codec_SelectedItem,
                                                        VM.AudioView.Audio_Quality_SelectedItem
                                                        ),
                            "\r\n\r\n" +
                            Audio.BatchAudioBitRateLimiter(VM.AudioView.Audio_Codec_SelectedItem,
                                                           VM.AudioView.Audio_Quality_SelectedItem
                                                           ),

                            "\r\n\r\n" +
                            "&&",
                            "\r\n\r\n" +
                            FFmpeg.Exe_InvokeOperator_PowerShell() +
                            MainWindow.FFmpegPath(),

                            "\r\n\r\n" +
                            "-y",
                   
                            // %~f added in InputPath()
                            //OnePass_CRF_Args(), // disabled if 2-Pass       
                            //TwoPass_Args() // disabled if 1-Pass/CRF
                        };

                        // Add Arguments to ffmpegBatchArgsList
                        switch (VM.VideoView.Video_Pass_SelectedItem)
                        {
                            // -------------------------
                            // CRF
                            // -------------------------
                            case "CRF":
                                ffmpegBatchArgsList.Add(CRF.Arguments());
                                break;

                            // -------------------------
                            // 1 Pass
                            // -------------------------
                            case "1 Pass":
                                ffmpegBatchArgsList.Add(_1_Pass.Arguments());
                                break;

                            // -------------------------
                            // 2 Pass
                            // -------------------------
                            case "2 Pass":
                                ffmpegBatchArgsList.Add(_2_Pass.Arguments());
                                break;

                            // -------------------------
                            // Empty, none, auto / Audio
                            // -------------------------
                            default:
                                ffmpegBatchArgsList.Add(_1_Pass.Arguments());
                                break;
                        }

                        break;

                    // -------------------------
                    // PowerShell
                    // -------------------------
                    case "PowerShell":
                        // Video Auto Quality Detect Bitrate
                        string vBitRateBatch = string.Empty;
                        if (VM.VideoView.Video_Quality_SelectedItem == "Auto")
                        {
                            vBitRateBatch = "\r\n" + "$vBitrate = " + FFmpeg.Exe_InvokeOperator_PowerShell() + FFprobe.ffprobe + " -v error -select_streams v:0 -show_entries " + FFprobe.vEntryTypeBatch + " -of default=noprint_wrappers=1:nokey=1 \"$fullName\"" + ";";
                        }

                        // Audio Auto Quality Detect Bitrate
                        string aBitRateBatch = string.Empty;
                        string aBitRateBatch_NullCheck = string.Empty;
                        string aBitRateBatch_Limited = string.Empty;
                        if (VM.AudioView.Audio_Quality_SelectedItem == "Auto")
                        {
                            aBitRateBatch = "\r\n" + "$aBitrate = " + FFmpeg.Exe_InvokeOperator_PowerShell() + FFprobe.ffprobe + " -v error -select_streams a:0 -show_entries " + FFprobe.aEntryType + " -of default=noprint_wrappers=1:nokey=1 \"$fullName\"" + ";";

                            // Bitrate Null Check
                            aBitRateBatch_NullCheck = "if (!$aBitrate) { $aBitrate = 0};";

                            // Bitrate Limiter
                            switch (VM.AudioView.Audio_Codec_SelectedItem)
                            {
                                // Vorbis
                                case "Vorbis":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 500000) { $aBitrate = 500000 };";
                                    break;

                                // Opus
                                case "Opus":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 510000) { $aBitrate = 510000 };";
                                    break;

                                // AAC
                                case "AAC":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 400000) { $aBitrate = 400000 };";
                                    break;

                                // AC3
                                case "AC3":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 640000) { $aBitrate = 640000 };";
                                    break;

                                // DTS
                                case "DTS":
                                    aBitRateBatch_Limited = "if ($aBitrate -lt 320000) { $aBitrate = 320000 }; if ($aBitrate -gt 1509075) { $aBitrate = 1509075 };";
                                    break;

                                // MP2
                                case "MP2":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 384000) { $aBitrate = 384000 };";
                                    break;

                                // LAME
                                case "LAME":
                                    aBitRateBatch_Limited = "if ($aBitrate -gt 320000) { $aBitrate = 320000 };";
                                    break;

                                    //// FLAC
                                    // Do not use, empty is FFmpeg default
                                    //case "FLAC":
                                    //    aBitRateBatch_Limited = "if ($aBitrate -gt 1411000) { $aBitrate = 1411000 };";
                                    //    break;

                                    //// PCM
                                    // Do not use, empty is FFmpeg default
                                    //case "PCM":
                                    //    aBitRateBatch_Limited = "if ($aBitrate -gt 1536000) { $aBitrate = 1536000 };";
                                    //    break;
                            }
                        }

                        // -------------------------
                        // Batch Arguments Full
                        // -------------------------
                        ffmpegBatchArgsList = new List<string>()
                        {
                            "$files = Get-ChildItem " + "\"" + MainWindow.BatchInputDirectory().TrimEnd('\\') + "\"" + " -Filter *" + MainWindow.inputExt, // trim the dir's end backslash
                            "\r\n\r\n" + ";",

                            "\r\n\r\n" + "foreach ($f in $files) {" +

                            "\r\n\r\n" + "$fullName = $f.FullName" + ";", // capture full path to variable
                            "\r\n" + "$name = $f.Name" + ";", // capture name to variable
                            vBitRateBatch, // capture video bitrate to variable
                            aBitRateBatch, // capture audio bitrate to variable
                            aBitRateBatch_NullCheck, // check if bitrate is null, change to 0
                            aBitRateBatch_Limited, // limit audio bitrate

                            "\r\n\r\n" +
                            FFmpeg.Exe_InvokeOperator_PowerShell() +
                            MainWindow.FFmpegPath(),
                            "\r\n\r\n" +
                            "-y",
                   
                            // $name added in InputPath()

                            //OnePass_CRF_Args(), // disabled if 2-Pass       
                            //TwoPass_Args(), // disabled if 1-Pass/CRF

                            //"\r\n\r\n" + "}"
                        };

                        // Add Arguments to ffmpegBatchArgsList
                        switch (VM.VideoView.Video_Pass_SelectedItem)
                        {
                            // -------------------------
                            // CRF
                            // -------------------------
                            case "CRF":
                                ffmpegBatchArgsList.Add(CRF.Arguments());
                                break;

                            // -------------------------
                            // 1 Pass
                            // -------------------------
                            case "1 Pass":
                                ffmpegBatchArgsList.Add(_1_Pass.Arguments());
                                break;

                            // -------------------------
                            // 2 Pass
                            // -------------------------
                            case "2 Pass":
                                ffmpegBatchArgsList.Add(_2_Pass.Arguments());
                                break;

                            // -------------------------
                            // Empty, none, auto / Audio
                            // -------------------------
                            default:
                                ffmpegBatchArgsList.Add(_1_Pass.Arguments());
                                break;
                        }

                        // Add Closing Bracket
                        ffmpegBatchArgsList.Add("\r\n\r\n" + "}");

                        break;
                }

                // Join List with Spaces
                // Remove: Empty, Null, Standalone LineBreak
                FFmpeg.ffmpegArgsSort = string.Join(" ", ffmpegBatchArgsList
                                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                                  .Where(s => !s.Equals(Environment.NewLine))
                                                  .Where(s => !s.Equals("\r\n\r\n"))
                                                  .Where(s => !s.Equals("\r\n"))
                                        );

                // Inline 
                FFmpeg.ffmpegArgs = MainWindow.RemoveLineBreaks(FFmpeg.ffmpegArgsSort);
            }
        }
    }
}
