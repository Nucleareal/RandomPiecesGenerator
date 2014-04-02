using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;

namespace RandomPiecesGenerator
{
    class Launcher
    {
        private static Cn Cns
        {
            set;
            get;
        }

        public static void Main(string[] args)
        {
            Cns = new Cn();

            bool isAvailableArgs = args.Length > 0;

            if (!isAvailableArgs)
            {
                PutUsage();
                return;
            }

            var opts = new List<string>();
            var names = new List<string>();

            bool isOptEnd = false;

            foreach (var v in args)
            {
                isOptEnd |= v.StartsWith("--");

                if (v.StartsWith("-") && !isOptEnd)
                {
                    opts.Add(v);
                }
                else
                {
                    names.Add(v);
                }
            }

            bool isShuffle = false;

            if (isAvailableArgs)
            {
                foreach (var v in opts)
                {
                    if (v.Contains("-q"))
                    {
                        Cns = new NCn();
                    }
                    if (v.Contains("-h"))
                    {
                        PutUsage();
                        return;
                    }
                    if (v.Contains("-r"))
                    {
                        isShuffle |= true;
                    }
                }
            }

            foreach(var v in names)
            {
                if (!File.Exists(v))
                {
                    Cns.Error("ImageFile {0} Is Not Exist", v);
                    continue;
                }

                using (Bitmap img = new Bitmap(v))
                {
                    var dvx = 16;
                    var dvy = 16;
                    var SC = 16;
                    var CostC = 300;
                    var CostS = 100;

                    var sps = Path.GetFileName(v).Split('_', '.');
                    if (sps.Length < 7)
                    {
                        Cns.Warning("FileName not Contains Params (You'll read Readme.txt), Use Default Params");
                    }
                    else
                    {
                        var cpt = new string[sps.Length - 2];

                        Array.Copy(sps, 1, cpt, 0, sps.Length - 2);

                        var toA = cpt.Select(int.Parse).ToArray();
                        dvx = toA[0];
                        dvy = toA[1];
                        SC = toA[2];
                        CostC = toA[3];
                        CostS = toA[4];
                    }

                    // Filename _ DVX _ DVY _ SetectableCount _ CostRateX _ CostRateY . 拡張子

                    string fname = Regex.Replace(v, @"(png|gif)", "ppm");
                    int w = img.Width;
                    int h = img.Height;

                    //Cns.WriteLine("({0}, {1})", w, h);

                    if (w % 16 != 0 || h % 16 != 0)
                    {
                        Cns.Warning("Width or Height should be multiples of 16");
                    }

                    using (StreamWriter sw = new StreamWriter(fname))
                    {
                        sw.WriteLine("P6");
                        sw.WriteLine("# {0} {1}", dvx, dvy);
                        sw.WriteLine("# {0}", SC);
                        sw.WriteLine("# {0} {1}", CostC, CostS);
                        sw.WriteLine("{0} {1}", w, h);
                        sw.WriteLine("255");
                    }

                    Shuffler sf = new Shuffler(dvx, dvy, SC, w / dvx, h / dvy);
                    sf.Init();

                    if (isShuffle)
                    {
                        sf.Shuffle();
                    }

                    //w = 1024
                    //h =  512

                    int dix = w / dvx;
                    int diy = h / dvy;

                    //dix = 1024 / 16 = 64

                    using (FileStream fs = new FileStream(fname, FileMode.Append, FileAccess.Write))
                    {
                        for (int j = 0; j < h; j++)
                        {
                            for (int i = 0; i < w; i++)
                            {
                                int inX = i / dix;
                                int inY = j / diy;

                                int lvX = sf[inX, inY].FromX;
                                int lvY = sf[inX, inY].FromY;

                                int toX = lvX * dix + i % dix;
                                int toY = lvY * diy + j % diy;

                                Color c = img.GetPixel(toX, toY);

                                fs.WriteByte(c.R);
                                fs.WriteByte(c.G);
                                fs.WriteByte(c.B);
                            }
                        }
                    }

                    //Cns.WriteLine("Yeah");
                }
            }
        }

        private static void PutUsage()
        {
            Cns.WriteLine(@"USAGE: .\{0} [Options] [FileNames]", Process.GetCurrentProcess().MainModule.ModuleName);
            Cns.WriteLine("\t-q: Quiet");
            Cns.WriteLine("\t-h: Help(this)");
            Cns.WriteLine("\t-r: Randomize");
            Cns.WriteLine("\t--: End of Option");
        }

        private class Cn
        {
            public virtual void WriteLine()
            {
                Console.WriteLine();
            }

            public virtual void WriteLine(object o)
            {
                Console.WriteLine(o);
            }

            public virtual void WriteLine(string str, params object[] obj)
            {
                Console.WriteLine(str, obj);
            }

            public virtual void Error(string str, params object[] obj)
            {
                WriteLine(string.Format("ERROR: {0}", str), obj);
            }

            public virtual void Warning(string str, params object[] obj)
            {
                WriteLine(string.Format("Warning: {0}", str), obj);
            }
        }

        private class NCn : Cn
        {
            public override void WriteLine()
            {
            }

            public override void WriteLine(string str, params object[] obj)
            {
            }
        }
    }
}
