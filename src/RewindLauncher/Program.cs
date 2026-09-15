using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.Json;

namespace RewindLauncher;

internal static class Program
{
    [STAThread]
    static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); }
}

public sealed class MainForm : Form
{
    static readonly Color Navy = Color.FromArgb(5, 14, 29);
    static readonly Color Side = Color.FromArgb(12, 25, 45);
    static readonly Color Card = Color.FromArgb(10, 24, 40);
    static readonly Color Cyan = Color.FromArgb(21, 190, 255);
    static readonly Color Muted = Color.FromArgb(146, 165, 190);
    readonly string dataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ThrowbackLauncher", "library.json");
    readonly Panel page = new() { Dock = DockStyle.Fill, BackColor = Navy };
    readonly Button play = new();
    readonly Label buildName = new();
    readonly Label buildPath = new();
    readonly Label status = new();
    readonly FlowLayoutPanel library = new();
    LibraryState state = new();
    Image? heroImage;\n    Image? logoImage;

    public MainForm()
    {
        Text = "Throwback Launcher";
        Size = new Size(1260, 760);
        MinimumSize = new Size(1040, 680);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Navy;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        try { using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ThrowbackLauncher.hero.png"); if (s is not null) heroImage = Image.FromStream(s); } catch { }\n        try { using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ThrowbackLauncher.logo.png"); if (s is not null) logoImage = Image.FromStream(s); } catch { }
        LoadState();
        BuildShell();
        ShowHome();
    }

    void BuildShell()
    {
        var side = new Panel { Dock = DockStyle.Left, Width = 252, BackColor = Side };
        var mark = new PictureBox { Image = logoImage, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Transparent, Size = new Size(50, 50), Location = new Point(17, 49) };
        var brand = new Label { Text = "REWIND\nLAUNCHER", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(78, 56) };
        side.Controls.Add(mark); side.Controls.Add(brand);

        var nav = new[] { ("⌂", "Home", (Action)ShowHome), ("▦", "Library", (Action)ShowLibrary), ("♕", "Leaderboard", (Action)ComingSoon), ("⚔", "Tournaments", (Action)ComingSoon), ("▣", "Shop", (Action)ComingSoon), ("↧", "Updates", (Action)ComingSoon), ("⚙", "Settings", (Action)ComingSoon) };
        var y = 132;
        foreach (var (icon, text, action) in nav)
        {
            var b = new Button { Text = icon + "    " + text, TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.Flat, ForeColor = text == "Home" ? Color.White : Color.FromArgb(176, 191, 213), BackColor = text == "Home" ? Color.FromArgb(24, 70, 106) : Side, Font = new Font("Segoe UI", 10, text == "Home" ? FontStyle.Bold : FontStyle.Regular), Size = new Size(230, 44), Location = new Point(10, y), Cursor = Cursors.Hand, Padding = new Padding(8, 0, 0, 0) };
            b.FlatAppearance.BorderColor = text == "Home" ? Color.FromArgb(27, 127, 181) : Side;
            b.Click += (_, _) => action();
            side.Controls.Add(b);
            y += text == "Updates" ? 68 : 44;
        }
        var account = new Panel { Height = 64, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(18, 35, 56), Padding = new Padding(14, 10, 10, 10) };
        account.Controls.Add(new Label { Text = "PI", BackColor = Cyan, ForeColor = Color.White, Size = new Size(38, 38), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(12, 12) });
        account.Controls.Add(new Label { Text = Environment.UserName + "\nLOCAL PLAYER", ForeColor = Color.White, AutoSize = true, Location = new Point(62, 14), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) });
        side.Controls.Add(account);

        var top = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Color.FromArgb(16, 42, 73) };
        status.Text = "●  OFFLINE";
        status.ForeColor = Color.FromArgb(255, 83, 101);
        status.BackColor = Color.FromArgb(23, 51, 83);
        status.BorderStyle = BorderStyle.FixedSingle;
        status.AutoSize = true;
        status.Padding = new Padding(10, 7, 10, 7);
        status.Location = new Point(24, 25);
        status.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        top.Controls.Add(status);
        var user = new Label { Text = Environment.UserName.ToUpperInvariant() + "    ●", ForeColor = Color.White, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(820, 32), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        top.Resize += (_, _) => user.Left = top.ClientSize.Width - user.Width - 28;
        top.Controls.Add(user);

        Controls.Add(page);
        Controls.Add(top);
        Controls.Add(side);
    }

    void ShowHome()
    {
        page.Controls.Clear();
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(32, 30, 32, 30), BackColor = Navy };
        var hero = new HeroPanel(heroImage) { Height = 290, Dock = DockStyle.Top, Padding = new Padding(34) };
        hero.Controls.Add(new Label { Text = "━  ACTIVE BUILD", ForeColor = Cyan, BackColor = Color.Transparent, AutoSize = true, Location = new Point(31, 98), Font = new Font("Segoe UI", 8, FontStyle.Bold) });
        buildName.ForeColor = Color.White; buildName.BackColor = Color.Transparent; buildName.AutoSize = true; buildName.Location = new Point(31, 124); buildName.Font = new Font("Segoe UI", 24, FontStyle.Bold);
        buildPath.ForeColor = Color.FromArgb(184, 198, 216); buildPath.BackColor = Color.Transparent; buildPath.AutoEllipsis = true; buildPath.Size = new Size(610, 22); buildPath.Location = new Point(34, 165); buildPath.Font = new Font("Consolas", 8.5f);
        hero.Controls.Add(buildName); hero.Controls.Add(buildPath);
        StyleButton(play, true); play.Size = new Size(150, 50); play.Location = new Point(34, 200); play.Click -= PlayClick; play.Click += PlayClick;
        var lib = MakeButton("LIBRARY", false); lib.Size = new Size(114, 50); lib.Location = new Point(196, 200); lib.Click += (_, _) => ShowLibrary();
        hero.Controls.Add(play); hero.Controls.Add(lib);

        var profile = new Panel { Size = new Size(235, 112), Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(22, 20, 37), Location = new Point(680, 142) };
        profile.Controls.Add(new Label { Text = "●  OFFLINE", ForeColor = Color.FromArgb(255, 83, 101), AutoSize = true, Location = new Point(18, 18), Font = new Font("Segoe UI", 8, FontStyle.Bold) });
        profile.Controls.Add(new Label { Text = "SIGNED IN\n" + Environment.UserName.ToUpperInvariant(), ForeColor = Color.White, AutoSize = true, Location = new Point(18, 54), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) });
        hero.Resize += (_, _) => profile.Left = hero.ClientSize.Width - profile.Width - 34;
        hero.Controls.Add(profile);

        var newsTitle = new Label { Text = "━  LATEST NEWS", Dock = DockStyle.Top, Height = 52, Padding = new Padding(0, 21, 0, 0), ForeColor = Cyan, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        var newsRow = new Panel { Dock = DockStyle.Top, Height = 235 };
        var newsImage = new PictureBox { Dock = DockStyle.Left, Width = 610, Image = heroImage, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Card };
        var newsCards = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 0, 0, 0) };
        var welcome = NewsCard("01", "WELCOME TO THROWBACK", "Your archive is ready. Pick a build and jump back in.", true);
        welcome.Dock = DockStyle.Top; welcome.Height = 105;
        var update = NewsCard("02", "BUILD IMPORTS", "Select Engine + FortniteGame to begin.", false);
        update.Dock = DockStyle.Bottom; update.Height = 105;
        newsCards.Controls.Add(welcome); newsCards.Controls.Add(update);
        newsRow.Controls.Add(newsCards); newsRow.Controls.Add(newsImage);

        scroll.Controls.Add(newsRow);
        scroll.Controls.Add(newsTitle);
        scroll.Controls.Add(hero);
        page.Controls.Add(scroll);
        RenderSelection();
    }

    Panel NewsCard(string number, string title, string text, bool active)
    {
        var p = new Panel { BackColor = Card, Padding = new Padding(16) };
        p.Paint += (_, e) => { using var pen = new Pen(active ? Cyan : Color.FromArgb(43, 61, 82), active ? 2 : 1); e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
        p.Controls.Add(new Label { Text = number, ForeColor = active ? Cyan : Color.FromArgb(96, 113, 135), AutoSize = true, Location = new Point(15, 18), Font = new Font("Segoe UI", 11, FontStyle.Bold | FontStyle.Italic) });
        p.Controls.Add(new Label { Text = title + "\n" + text, ForeColor = Color.White, AutoSize = true, Location = new Point(53, 17), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) });
        return p;
    }

    void ShowLibrary()
    {
        page.Controls.Clear();
        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(32), BackColor = Navy };
        var header = new Panel { Dock = DockStyle.Top, Height = 75 };
        header.Controls.Add(new Label { Text = "BUILD LIBRARY", ForeColor = Color.White, AutoSize = true, Location = new Point(0, 6), Font = new Font("Segoe UI", 22, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "Import and manage locally owned game builds.", ForeColor = Muted, AutoSize = true, Location = new Point(2, 45) });
        var import = MakeButton("＋ IMPORT BUILD", true); import.Size = new Size(160, 42); import.Anchor = AnchorStyles.Top | AnchorStyles.Right; import.Location = new Point(760, 10); import.Click += (_, _) => ImportBuild();
        header.Resize += (_, _) => import.Left = header.ClientSize.Width - import.Width;
        header.Controls.Add(import);
        library.Dock = DockStyle.Fill; library.AutoScroll = true; library.WrapContents = true; library.BackColor = Navy;
        wrap.Controls.Add(library); wrap.Controls.Add(header); page.Controls.Add(wrap);
        RenderLibrary();
    }

    void RenderSelection()
    {
        var b = state.Builds.FirstOrDefault(x => x.Id == state.SelectedId);
        buildName.Text = b?.Name.ToUpperInvariant() ?? "NO BUILD SELECTED";
        buildPath.Text = b?.Root ?? "Open Library and import a build folder";
        play.Enabled = b is not null;
        play.Text = b is null ? "▶  SELECT" : "▶  PLAY";
    }

    void RenderLibrary()
    {
        library.Controls.Clear();
        foreach (var b in state.Builds)
        {
            var selected = b.Id == state.SelectedId;
            var card = new Panel { Width = 285, Height = 155, BackColor = selected ? Color.FromArgb(19, 51, 75) : Card, Margin = new Padding(0, 0, 14, 14), Cursor = Cursors.Hand };
            card.Paint += (_, e) => { using var pen = new Pen(selected ? Cyan : Color.FromArgb(39, 59, 81), selected ? 2 : 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            var badge = new Label { Text = "●  LOCAL BUILD", ForeColor = Color.FromArgb(68, 229, 156), AutoSize = true, Location = new Point(17, 18), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            var name = new Label { Text = b.Name, ForeColor = Color.White, AutoEllipsis = true, Size = new Size(250, 30), Location = new Point(16, 55), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            var ver = new Label { Text = b.Version, ForeColor = Muted, AutoSize = true, Location = new Point(17, 88) };
            var remove = MakeButton("REMOVE", false); remove.Size = new Size(78, 29); remove.Location = new Point(190, 112); remove.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold); remove.Click += (_, _) => RemoveBuild(b);
            void choose(object? _, EventArgs __) { state.SelectedId = b.Id; SaveState(); RenderLibrary(); }
            card.Click += choose; badge.Click += choose; name.Click += choose; ver.Click += choose;
            card.Controls.AddRange([badge, name, ver, remove]); library.Controls.Add(card);
        }
        if (state.Builds.Count == 0) library.Controls.Add(new Label { Text = "No builds imported.\n\nClick IMPORT BUILD and choose the folder containing Engine and FortniteGame.", ForeColor = Muted, BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleCenter, Size = new Size(640, 125) });
    }

    void ImportBuild()
    {
        using var folder = new FolderBrowserDialog { Description = "Select the folder containing Engine and FortniteGame", UseDescriptionForTitle = true };
        if (folder.ShowDialog(this) != DialogResult.OK) return;
        var root = folder.SelectedPath;
        var engine = Path.Combine(root, "Engine");
        var game = Path.Combine(root, "FortniteGame");
        var exe = Path.Combine(game, "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
        if (!Directory.Exists(engine) || !Directory.Exists(game)) { MessageBox.Show(this, "That folder must contain both Engine and FortniteGame.", "Invalid build folder", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!File.Exists(exe)) { MessageBox.Show(this, "The Windows shipping client was not found at:\n\n" + exe, "Client not found", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var name = new DirectoryInfo(root).Name; var version = ""; ReadManifest(root, ref name, ref version);
        using var prompt = new ImportDialog(name, version, root);
        if (prompt.ShowDialog(this) != DialogResult.OK) return;
        var build = new BuildEntry { Id = Guid.NewGuid().ToString("N"), Name = prompt.BuildName, Version = prompt.BuildVersion, Root = root, Exe = exe };
        state.Builds.RemoveAll(x => string.Equals(x.Root, root, StringComparison.OrdinalIgnoreCase));
        state.Builds.Insert(0, build); state.SelectedId = build.Id; SaveState(); ShowHome();
    }

    static void ReadManifest(string root, ref string name, ref string version)
    {
        foreach (var file in new[] { "throwback-manifest.json", "manifest.json" })
        {
            var path = Path.Combine(root, file); if (!File.Exists(path)) continue;
            try { using var doc = JsonDocument.Parse(File.ReadAllText(path)); name = Find(doc.RootElement, ["name", "displayName", "seasonName"]) ?? name; version = Find(doc.RootElement, ["version", "buildVersion", "gameVersion"]) ?? version; } catch { }
            break;
        }
    }

    static string? Find(JsonElement value, string[] names)
    {
        if (value.ValueKind == JsonValueKind.Object) foreach (var p in value.EnumerateObject()) { if (names.Any(n => string.Equals(n, p.Name, StringComparison.OrdinalIgnoreCase)) && p.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number) return p.Value.ToString(); var nested = Find(p.Value, names); if (!string.IsNullOrWhiteSpace(nested)) return nested; }
        else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) { var nested = Find(item, names); if (!string.IsNullOrWhiteSpace(nested)) return nested; }
        return null;
    }

    void PlayClick(object? sender, EventArgs e) => LaunchSelected();
    void LaunchSelected()
    {
        var b = state.Builds.FirstOrDefault(x => x.Id == state.SelectedId); if (b is null) return;
        if (!File.Exists(b.Exe)) { MessageBox.Show(this, "The executable is missing. Re-import the build if its folder moved.", "Client not found", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (MessageBox.Show(this, $"Launch this local client?\n\n{b.Name} ({b.Version})\n{b.Exe}", "Confirm launch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { Process.Start(new ProcessStartInfo { FileName = b.Exe, WorkingDirectory = Path.GetDirectoryName(b.Exe)!, UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(this, "Launch failed:\n" + ex.Message, "Launch failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void RemoveBuild(BuildEntry b)
    {
        if (MessageBox.Show(this, "Remove this launcher entry? No game files will be deleted.", "Remove build", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        state.Builds.Remove(b); if (state.SelectedId == b.Id) state.SelectedId = ""; SaveState(); RenderLibrary();
    }

    void UpdateClientStatus()\n    {\n        var b = state.Builds.FirstOrDefault(x => x.Id == state.SelectedId);\n        var running = false;\n        try { if (b is not null) running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(b.Exe)).Length > 0; } catch { }\n        status.Text = running ? "●  IN GAME" : "●  OFFLINE";\n        status.ForeColor = running ? Color.FromArgb(68, 229, 156) : Color.FromArgb(255, 83, 101);\n    }\n\n    void ComingSoon() => MessageBox.Show(this, "This section is ready for a future update.", "Throwback Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
    Button MakeButton(string text, bool primary) { var b = new Button { Text = text }; StyleButton(b, primary); return b; }
    void StyleButton(Button b, bool primary) { b.FlatStyle = FlatStyle.Flat; b.Cursor = Cursors.Hand; b.ForeColor = Color.White; b.BackColor = primary ? Color.FromArgb(10, 132, 236) : Color.FromArgb(24, 34, 54); b.Font = new Font("Segoe UI", 9, FontStyle.Bold); b.FlatAppearance.BorderColor = primary ? Cyan : Color.FromArgb(61, 73, 94); }
    void LoadState() { try { if (File.Exists(dataFile)) state = JsonSerializer.Deserialize<LibraryState>(File.ReadAllText(dataFile)) ?? new(); } catch { state = new(); } }
    void SaveState() { Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!); File.WriteAllText(dataFile, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true })); }
}

public sealed class HeroPanel : Panel
{
    readonly Image? image;
    public HeroPanel(Image? image) { this.image = image; DoubleBuffered = true; BackColor = Color.FromArgb(9, 22, 38); }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        if (image is not null) e.Graphics.DrawImage(image, ClientRectangle);
        using var shade = new LinearGradientBrush(ClientRectangle, Color.FromArgb(235, 4, 14, 27), Color.FromArgb(50, 4, 14, 27), LinearGradientMode.Horizontal);
        e.Graphics.FillRectangle(shade, ClientRectangle);
        using var pen = new Pen(Color.FromArgb(46, 88, 123)); e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}

public sealed class ImportDialog : Form
{
    readonly TextBox name = new(), version = new();
    public string BuildName => name.Text.Trim();
    public string BuildVersion => version.Text.Trim();
    public ImportDialog(string initialName, string initialVersion, string root)
    {
        Text = "Import Build"; Size = new Size(520, 330); StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; BackColor = Color.FromArgb(8, 20, 35); ForeColor = Color.White; Font = new Font("Segoe UI", 10);
        Controls.Add(new Label { Text = "Build folder", Location = new Point(24, 22), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        Controls.Add(new Label { Text = root + "\n✓ Windows client found", Location = new Point(24, 46), Size = new Size(450, 45), ForeColor = Color.FromArgb(175, 195, 216) });
        Controls.Add(new Label { Text = "Build name", Location = new Point(24, 100), AutoSize = true }); name.SetBounds(24, 125, 450, 31); name.Text = initialName; name.BackColor = Color.FromArgb(4, 12, 23); name.ForeColor = Color.White; Controls.Add(name);
        Controls.Add(new Label { Text = "Version", Location = new Point(24, 170), AutoSize = true }); version.SetBounds(24, 195, 450, 31); version.Text = initialVersion; version.BackColor = Color.FromArgb(4, 12, 23); version.ForeColor = Color.White; Controls.Add(version);
        var cancel = new Button { Text = "CANCEL", DialogResult = DialogResult.Cancel, Location = new Point(274, 245), Size = new Size(95, 38), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(24, 34, 54) };
        var save = new Button { Text = "IMPORT", Location = new Point(379, 245), Size = new Size(95, 38), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(10, 132, 236) };
        save.Click += (_, _) => { if (BuildName.Length == 0 || BuildVersion.Length == 0) MessageBox.Show(this, "Enter a name and version."); else DialogResult = DialogResult.OK; };
        Controls.Add(cancel); Controls.Add(save); AcceptButton = save; CancelButton = cancel;
    }
}

public sealed class LibraryState { public string SelectedId { get; set; } = ""; public List<BuildEntry> Builds { get; set; } = []; }
public sealed class BuildEntry { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Version { get; set; } = ""; public string Root { get; set; } = ""; public string Exe { get; set; } = ""; }
