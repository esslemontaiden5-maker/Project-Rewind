using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace RewindLauncher;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly Color Bg = Color.FromArgb(8, 5, 16);
    private readonly Color Panel = Color.FromArgb(20, 13, 37);
    private readonly Color Purple = Color.FromArgb(176, 92, 255);
    private readonly string dataFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RewindLauncher", "library.json");

    private readonly FlowLayoutPanel cards = new() { AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
    private readonly Button play = new();
    private readonly Label count = new();
    private LibraryState state = new();

    public MainForm()
    {
        Text = "Rewind Launcher";
        MinimumSize = new Size(980, 680);
        Size = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        BuildUi();
        LoadState();
        RenderCards();
    }

    private void BuildUi()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(12, 7, 24), Padding = new Padding(18) };
        var logo = new Label { Text = "R", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(124, 58, 237), TextAlign = ContentAlignment.MiddleCenter, Size = new Size(48, 48), Location = new Point(18, 24) };
        var brand = new Label { Text = "REWIND\nLAUNCHER", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(78, 27) };
        var nav = new Label { Text = "⌂   BUILD LIBRARY", ForeColor = Color.White, BackColor = Color.FromArgb(54, 32, 88), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 48, Dock = DockStyle.Top, Padding = new Padding(12, 0, 0, 0), Margin = new Padding(0, 100, 0, 0) };
        var disclaimer = new Label { Text = "LOCAL FILES ONLY\n\nIndependent fan-made launcher.\nNot affiliated with Epic Games.", ForeColor = Color.FromArgb(125, 110, 145), Dock = DockStyle.Bottom, Height = 105, Font = new Font("Segoe UI", 8.5f) };
        sidebar.Controls.Add(disclaimer);
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(brand);
        sidebar.Controls.Add(logo);
        nav.Location = new Point(18, 110);
        nav.Width = 184;

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), BackColor = Bg };
        var top = new Panel { Dock = DockStyle.Top, Height = 54 };
        top.Controls.Add(new Label { Text = "●  SYSTEM READY", ForeColor = Color.FromArgb(80, 225, 160), AutoSize = true, Location = new Point(0, 15), Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        var importTop = MakeButton("＋  IMPORT BUILD", false);
        importTop.Size = new Size(160, 40);
        importTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        importTop.Location = new Point(top.Width - 160, 0);
        top.Resize += (_, _) => importTop.Left = top.ClientSize.Width - importTop.Width;
        importTop.Click += (_, _) => ImportBuild();
        top.Controls.Add(importTop);

        var hero = new GradientPanel { Dock = DockStyle.Top, Height = 245, Padding = new Padding(34), Start = Color.FromArgb(18, 8, 36), End = Color.FromArgb(74, 28, 116) };
        var eyebrow = new Label { Text = "LOCAL BUILD MANAGER", ForeColor = Purple, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(34, 30) };
        var title = new Label { Text = "DROP IN.\nREWIND TIME.", ForeColor = Color.White, AutoSize = true, Font = new Font("Segoe UI", 29, FontStyle.Bold), Location = new Point(30, 52) };
        var sub = new Label { Text = "Import a locally owned build folder, select it, and launch the detected Windows client.", ForeColor = Color.FromArgb(206, 191, 222), AutoSize = true, Location = new Point(34, 138) };
        play.Text = "▶  SELECT A BUILD";
        StyleButton(play, true);
        play.Enabled = false;
        play.Location = new Point(34, 177);
        play.Size = new Size(260, 45);
        play.Click += (_, _) => LaunchSelected();
        var importHero = MakeButton("＋  IMPORT BUILD", false);
        importHero.Location = new Point(306, 177);
        importHero.Size = new Size(165, 45);
        importHero.Click += (_, _) => ImportBuild();
        hero.Controls.AddRange([eyebrow, title, sub, play, importHero]);

        var section = new Panel { Dock = DockStyle.Top, Height = 72 };
        section.Controls.Add(new Label { Text = "YOUR ARCHIVE", ForeColor = Purple, AutoSize = true, Location = new Point(0, 19), Font = new Font("Segoe UI", 8, FontStyle.Bold) });
        section.Controls.Add(new Label { Text = "INSTALLED BUILDS", ForeColor = Color.White, AutoSize = true, Location = new Point(-2, 37), Font = new Font("Segoe UI", 17, FontStyle.Bold) });
        count.ForeColor = Color.FromArgb(155, 140, 175);
        count.AutoSize = true;
        count.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        count.Location = new Point(section.Width - 80, 40);
        section.Resize += (_, _) => count.Left = section.ClientSize.Width - count.Width;
        section.Controls.Add(count);

        cards.Dock = DockStyle.Fill;
        cards.BackColor = Bg;
        cards.Padding = new Padding(0, 3, 0, 0);

        content.Controls.Add(cards);
        content.Controls.Add(section);
        content.Controls.Add(hero);
        content.Controls.Add(top);
        Controls.Add(content);
        Controls.Add(sidebar);
    }

    private Button MakeButton(string text, bool primary)
    {
        var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, ForeColor = Color.White, BackColor = primary ? Color.FromArgb(124, 58, 237) : Color.FromArgb(29, 18, 48), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        b.FlatAppearance.BorderColor = primary ? Purple : Color.FromArgb(64, 44, 91);
        return b;
    }

    private void StyleButton(Button b, bool primary)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.Cursor = Cursors.Hand;
        b.ForeColor = Color.White;
        b.BackColor = primary ? Color.FromArgb(124, 58, 237) : Panel;
        b.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        b.FlatAppearance.BorderColor = primary ? Purple : Color.FromArgb(64, 44, 91);
    }

    private void ImportBuild()
    {
        using var folder = new FolderBrowserDialog { Description = "Select the folder containing Engine and FortniteGame", UseDescriptionForTitle = true };
        if (folder.ShowDialog(this) != DialogResult.OK) return;

        var root = folder.SelectedPath;
        var engine = Path.Combine(root, "Engine");
        var game = Path.Combine(root, "FortniteGame");
        var exe = Path.Combine(game, "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");

        if (!Directory.Exists(engine) || !Directory.Exists(game))
        {
            MessageBox.Show(this, "That folder must contain both Engine and FortniteGame.", "Invalid build folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!File.Exists(exe))
        {
            MessageBox.Show(this, "The Windows shipping client was not found at:\n\n" + exe, "Client not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var name = new DirectoryInfo(root).Name;
        var version = "";
        ReadManifest(root, ref name, ref version);

        using var prompt = new ImportDialog(name, version, root);
        if (prompt.ShowDialog(this) != DialogResult.OK) return;

        var build = new BuildEntry { Id = Guid.NewGuid().ToString("N"), Name = prompt.BuildName, Version = prompt.BuildVersion, Root = root, Exe = exe };
        state.Builds.RemoveAll(x => string.Equals(x.Root, root, StringComparison.OrdinalIgnoreCase));
        state.Builds.Insert(0, build);
        state.SelectedId = build.Id;
        SaveState();
        RenderCards();
    }

    private static void ReadManifest(string root, ref string name, ref string version)
    {
        foreach (var file in new[] { "throwback-manifest.json", "manifest.json" })
        {
            var path = Path.Combine(root, file);
            if (!File.Exists(path)) continue;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                name = FindString(doc.RootElement, ["name", "displayName", "seasonName"]) ?? name;
                version = FindString(doc.RootElement, ["version", "buildVersion", "gameVersion"]) ?? version;
            }
            catch { }
            break;
        }
    }

    private static string? FindString(JsonElement value, string[] names)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in value.EnumerateObject())
            {
                if (names.Any(n => string.Equals(n, p.Name, StringComparison.OrdinalIgnoreCase)) && p.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
                    return p.Value.ToString();
                var nested = FindString(p.Value, names);
                if (!string.IsNullOrWhiteSpace(nested)) return nested;
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var item in value.EnumerateArray())
            {
                var nested = FindString(item, names);
                if (!string.IsNullOrWhiteSpace(nested)) return nested;
            }
        return null;
    }

    private void RenderCards()
    {
        cards.SuspendLayout();
        cards.Controls.Clear();
        foreach (var b in state.Builds)
        {
            var card = new Panel { Width = 278, Height = 145, Margin = new Padding(0, 0, 12, 12), BackColor = b.Id == state.SelectedId ? Color.FromArgb(46, 24, 72) : Panel, Cursor = Cursors.Hand };
            card.Paint += (_, e) => { using var pen = new Pen(b.Id == state.SelectedId ? Purple : Color.FromArgb(57, 38, 86), b.Id == state.SelectedId ? 2 : 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            var badge = new Label { Text = "●  LOCAL BUILD", ForeColor = Color.FromArgb(90, 230, 165), BackColor = Color.FromArgb(17, 45, 35), AutoSize = true, Location = new Point(15, 15), Padding = new Padding(6, 4, 6, 4), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            var name = new Label { Text = b.Name, ForeColor = Color.White, AutoEllipsis = true, Width = 245, Height = 28, Location = new Point(15, 58), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            var ver = new Label { Text = b.Version, ForeColor = Color.FromArgb(160, 145, 180), AutoSize = true, Location = new Point(16, 88) };
            var remove = MakeButton("REMOVE", false);
            remove.Size = new Size(77, 28);
            remove.Location = new Point(185, 107);
            remove.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            remove.Click += (_, _) => RemoveBuild(b);
            void choose(object? _, EventArgs __) { state.SelectedId = b.Id; SaveState(); RenderCards(); }
            card.Click += choose; badge.Click += choose; name.Click += choose; ver.Click += choose;
            card.Controls.AddRange([badge, name, ver, remove]);
            cards.Controls.Add(card);
        }
        if (state.Builds.Count == 0)
            cards.Controls.Add(new Label { Text = "No builds imported.\n\nSelect the parent folder containing Engine and FortniteGame.", ForeColor = Color.FromArgb(155, 140, 175), BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleCenter, Width = 600, Height = 110, Margin = new Padding(0, 3, 0, 0) });

        count.Text = state.Builds.Count + (state.Builds.Count == 1 ? " build" : " builds");
        var selected = state.Builds.FirstOrDefault(x => x.Id == state.SelectedId);
        play.Enabled = selected is not null;
        play.Text = selected is null ? "▶  SELECT A BUILD" : "▶  PLAY " + selected.Name.ToUpperInvariant();
        cards.ResumeLayout();
    }

    private void RemoveBuild(BuildEntry build)
    {
        if (MessageBox.Show(this, "Remove this launcher entry? No game files will be deleted.", "Remove build", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        state.Builds.Remove(build);
        if (state.SelectedId == build.Id) state.SelectedId = "";
        SaveState();
        RenderCards();
    }

    private void LaunchSelected()
    {
        var b = state.Builds.FirstOrDefault(x => x.Id == state.SelectedId);
        if (b is null) return;
        if (!File.Exists(b.Exe))
        {
            MessageBox.Show(this, "The executable is missing. Re-import the build if its folder was moved.", "Client not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var result = MessageBox.Show(this, $"Launch this local client?\n\n{b.Name} ({b.Version})\n{b.Exe}", "Confirm launch", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = b.Exe, WorkingDirectory = Path.GetDirectoryName(b.Exe)!, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Launch failed:\n" + ex.Message, "Launch failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadState()
    {
        try { if (File.Exists(dataFile)) state = JsonSerializer.Deserialize<LibraryState>(File.ReadAllText(dataFile)) ?? new(); }
        catch { state = new(); }
    }

    private void SaveState()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!);
        File.WriteAllText(dataFile, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public sealed class GradientPanel : Panel
{
    public Color Start { get; set; }
    public Color End { get; set; }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new LinearGradientBrush(ClientRectangle, Start, End, 15f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

public sealed class ImportDialog : Form
{
    private readonly TextBox name = new();
    private readonly TextBox version = new();
    public string BuildName => name.Text.Trim();
    public string BuildVersion => version.Text.Trim();

    public ImportDialog(string initialName, string initialVersion, string root)
    {
        Text = "Import Build";
        Size = new Size(520, 330);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(18, 11, 33);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "Build folder", Location = new Point(24, 22), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        Controls.Add(new Label { Text = root + "\n✓ Windows client found", Location = new Point(24, 46), Size = new Size(450, 45), ForeColor = Color.FromArgb(180, 165, 200) });
        Controls.Add(new Label { Text = "Build name", Location = new Point(24, 100), AutoSize = true });
        name.SetBounds(24, 125, 450, 31); name.Text = initialName; name.BackColor = Color.FromArgb(9, 5, 17); name.ForeColor = Color.White; name.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(name);
        Controls.Add(new Label { Text = "Version", Location = new Point(24, 170), AutoSize = true });
        version.SetBounds(24, 195, 450, 31); version.Text = initialVersion; version.BackColor = Color.FromArgb(9, 5, 17); version.ForeColor = Color.White; version.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(version);
        var cancel = new Button { Text = "CANCEL", DialogResult = DialogResult.Cancel, Location = new Point(274, 245), Size = new Size(95, 38), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(29, 18, 48) };
        var save = new Button { Text = "IMPORT", Location = new Point(379, 245), Size = new Size(95, 38), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(124, 58, 237) };
        save.Click += (_, _) => { if (BuildName.Length == 0 || BuildVersion.Length == 0) MessageBox.Show(this, "Enter a name and version."); else DialogResult = DialogResult.OK; };
        Controls.Add(cancel); Controls.Add(save); AcceptButton = save; CancelButton = cancel;
    }
}

public sealed class LibraryState
{
    public string SelectedId { get; set; } = "";
    public List<BuildEntry> Builds { get; set; } = [];
}

public sealed class BuildEntry
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Root { get; set; } = "";
    public string Exe { get; set; } = "";
}
