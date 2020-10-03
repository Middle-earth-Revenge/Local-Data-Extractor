/*  LOTRO/DDO LOCAL DATA EXCTRACTOR
    Copyright(C) 2011-2012 Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LOTROLOCALDATA
{
    static class Program
    {
        /// <summary>
        /// Main entry
        /// </summary>
        static void Main()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("LOCALDATAEXTRACTOR {0}.{1} written by Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)", version.Major, version.Minor);

            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());

            if (files.Length == 0)
            {
                Console.WriteLine("No files were found. Please move this programme into the folder with localization .bin files!");
                return;
            }

            DateTime start = DateTime.Now;

            int count;
            var dic = ParseFiles(files, out count);

            Console.Write("Saving texts...");

            using (var sw = new StreamWriter("LocalData.txt"))
            {
                foreach (KeyValuePair<string, List<string>> kvp in dic)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        sw.WriteLine("{0} - {1}", kvp.Key + i.ToString("X2"), kvp.Value[i]);
                    }
                }
            }

            DateTime end = DateTime.Now;

            Console.WriteLine("done");
            Console.WriteLine("{0} texts were extracted in {1} seconds.", count, (int)(end - start).TotalSeconds);
        }

        /// <summary>
        /// Localization files parsing
        /// </summary>
        /// <param name="files">List of files</param>
        /// <param name="count">Total strings in files</param>
        /// <returns>Dictionary of localized strings</returns>
        public static Dictionary<string, List<string>> ParseFiles(string[] files, out int count)
        {
            Console.Write("Loading data...");

            var dic = new Dictionary<string, List<string>>();
            count = 0;

            foreach (var file in files)
            {
                if (!file.EndsWith(".bin"))
                    continue;

                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        var strings = new List<string>();
                        int id = br.ReadInt32();

                        br.ReadInt32(); // 1
                        byte type = br.ReadByte();

                        short c = br.ReadByte();

                        if ((c & 0x80) != 0)
                            c = (short)((c ^ 0x80) << 8 | br.ReadByte());

                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            long hash = br.ReadInt64();
                            int tCount = br.ReadInt32();

                            for (int j = 0; j < tCount; j++)
                            {
                                short len = br.ReadByte();

                                if ((len & 0x80) != 0)
                                    len = (short)((len ^ 0x80) << 8 | br.ReadByte());

                                byte[] chars = br.ReadBytes(len * 2);
                                string txt = Encoding.Unicode.GetString(chars);

                                strings.Add(string.Format("{0}", txt));
                                count++;
                            }

                            int uCount = br.ReadInt32();

                            for (int k = 0; k < uCount; k++)
                            {
                                int unkI = br.ReadInt32(); // unk
                            }

                            byte unk = br.ReadByte();

                            for (int t = 0; t < unk; t++)
                            {
                                int xCount = br.ReadInt32();

                                for (int z = 0; z < xCount; z++)
                                {
                                    short len = br.ReadByte();

                                    if ((len & 0x80) != 0)
                                        len = (short)((len ^ 0x80) << 8 | br.ReadByte());

                                    byte[] chars = br.ReadBytes(len * 2);

                                    string txt = Encoding.Unicode.GetString(chars);

                                    strings.Add(string.Format("{0}", txt));
                                    count++;
                                }
                            }
                        }

                        strings.Add("################################################################################");

                        if (!dic.ContainsKey(id.ToString("X2")))
                            dic.Add(id.ToString("X2"), strings);
                    }
                }
            }

            Console.WriteLine("done");

            return dic;
        }
    }
}