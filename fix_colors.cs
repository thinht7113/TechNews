using System;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        var baseDir = @""d:\BT\C#\Antigravity\Core\TechNews\TechNews.Web"";
        string[] exts = { ""*.cshtml"", ""*.js"" };
        foreach(var ext in exts) {
            foreach(var f in Directory.GetFiles(baseDir, ext, SearchOption.AllDirectories)) {
                var txt = File.ReadAllText(f, Encoding.UTF8);
                bool changed = false;
                if(txt.Contains(""#9f224e"")) { txt = txt.Replace(""#9f224e"", ""#2563eb""); changed = true; }
                if(txt.Contains(""#851b41"")) { txt = txt.Replace(""#851b41"", ""#1d4ed8""); changed = true; }
                if(txt.Contains(""#801b3e"")) { txt = txt.Replace(""#801b3e"", ""#1d4ed8""); changed = true; }
                if(changed) File.WriteAllText(f, txt, Encoding.UTF8);
            }
        }
    }
}
