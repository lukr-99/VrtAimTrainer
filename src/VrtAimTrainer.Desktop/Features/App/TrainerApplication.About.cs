using Raylib_cs;
using VrtAimTrainer.Core.Features.About;

namespace VrtAimTrainer.Desktop.Features.App;

/// <summary>
/// The About page: app identity, the author card, a feedback invitation with links, and credits.
/// Self-contained on purpose. It reads only the shell colour names (Accent, Card, ...) so it follows
/// whatever palette or theme the shell uses. It draws static content, so there is no motion to reduce.
/// </summary>
public sealed partial class TrainerApplication
{
    // Raylib's default font has no diacritics, so the page loads its own face for the author name.
    private static readonly int[] AboutCodepoints =
        Enumerable.Range(32, 95).Concat([0xE1, 0x10D, 0xED, 0x160, 0x161]).ToArray();

    private Font _aboutFont;
    private bool _aboutFontLoaded;
    private bool _aboutFontUsable;
    private float _aboutScroll;
    private float _aboutContentHeight;

    private void DrawAboutPage(Rectangle bounds)
    {
        EnsureAboutFont();
        DrawSectionHeader(bounds, "About");

        var viewport = new Rectangle(bounds.X, bounds.Y + 36f, bounds.Width, bounds.Height - 36f);
        var maxScroll = Math.Max(0f, _aboutContentHeight - viewport.Height);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), viewport))
        {
            _aboutScroll -= Raylib.GetMouseWheelMove() * 40f;
        }

        _aboutScroll = Math.Clamp(_aboutScroll, 0f, maxScroll);

        Raylib.BeginScissorMode((int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height);
        var cursor = new VerticalFlowCursor(
            new Rectangle(viewport.X, viewport.Y - _aboutScroll, viewport.Width, viewport.Height));
        var startY = cursor.CurrentY;

        DrawAboutHero(cursor.Take(88f));
        cursor.Space(ControlHubGap);
        DrawAboutAuthorCard(cursor.Take(212f));
        cursor.Space(ControlHubGap);
        DrawAboutLinks(cursor, viewport);
        cursor.Space(ControlHubGap);
        DrawAboutCredits(cursor);

        _aboutContentHeight = cursor.CurrentY - startY + 8f;
        Raylib.EndScissorMode();
    }

    private void DrawAboutHero(Rectangle bounds)
    {
        DrawPanelShape(bounds, CardAlt);
        Raylib.DrawRectangleLinesEx(bounds, 1.5f, CardBorder);
        DrawLabel(AppAbout.AppName, bounds.X + 16f, bounds.Y + 12f, 30, Text);
        DrawLabel($"Version {AppVersionProvider.DisplayVersion}", bounds.X + 16f, bounds.Y + 46f, 16, Accent);
        DrawWrappedLabel(AppAbout.Tagline, bounds.X + 16f, bounds.Y + 66f, bounds.Width - 32f, 14, MutedText, 1);
    }

    private void DrawAboutAuthorCard(Rectangle bounds)
    {
        DrawPanelShape(bounds, Card);
        Raylib.DrawRectangleLinesEx(bounds, 2f, Accent);
        Raylib.DrawRectangleRec(new Rectangle(bounds.X, bounds.Y, 6f, bounds.Height), Accent);

        var x = bounds.X + 24f;
        var width = bounds.Width - 40f;
        DrawLabel("MADE BY", x, bounds.Y + 16f, 14, Accent);

        var name = _aboutFontUsable ? AppAbout.AuthorName : AppAbout.AuthorNameAscii;
        var nameSize = 34;
        while (nameSize > 20 && MeasureAboutText(name, nameSize) > width)
        {
            nameSize -= 2;
        }

        DrawAboutText(name, x, bounds.Y + 38f, nameSize, Text);
        Raylib.DrawRectangleRec(new Rectangle(x, bounds.Y + 82f, 56f, 3f), Accent);
        DrawWrappedLabel(AppAbout.Summary, x, bounds.Y + 98f, width, 16, MutedText, 3);
        DrawWrappedLabel(AppAbout.FeedbackInvitation, x, bounds.Y + 160f, width, 16, Text, 2);
    }

    private void DrawAboutLinks(VerticalFlowCursor cursor, Rectangle viewport)
    {
        foreach (var link in AppAbout.Links)
        {
            var row = cursor.Take(46f);
            var visible = Raylib.CheckCollisionRecs(row, viewport);
            var isPrimary = link.Id == "issues";
            if (visible
                && _ui.Button($"about-{link.Id}", row, $"{link.Label}   {link.Handle}", isPrimary ? Accent : CardAlt, Accent, isPrimary ? Background : Text))
            {
                Raylib.OpenURL(link.Url);
            }

            cursor.Space(8f);
        }
    }

    private void DrawAboutCredits(VerticalFlowCursor cursor)
    {
        DrawLabel("Credits and licences", cursor.X, cursor.CurrentY, 22, Text);
        cursor.Space(34f);

        foreach (var credit in AppAbout.Credits)
        {
            var roleHeight = MeasureWrappedTextHeight(credit.Role, 14, cursor.Width - 28f, 3);
            var card = cursor.Take(46f + roleHeight);
            DrawPanelShape(card, CardAlt);
            Raylib.DrawRectangleLinesEx(card, 1f, CardBorder);
            DrawLabel(credit.Name, card.X + 14f, card.Y + 10f, 18, Text);
            DrawLabel(credit.License, card.Right() - 14f - Raylib.MeasureText(credit.License, 14), card.Y + 14f, 14, Accent);
            DrawWrappedLabel(credit.Role, card.X + 14f, card.Y + 36f, card.Width - 28f, 14, MutedText, 3);
            cursor.Space(8f);
        }

        DrawWrappedLabel(
            "Aim and reaction data stay on this computer. Nothing is sent anywhere.",
            cursor.X,
            cursor.CurrentY + 4f,
            cursor.Width,
            14,
            MutedText,
            2);
        cursor.Space(44f);
    }

    private void EnsureAboutFont()
    {
        if (_aboutFontLoaded)
        {
            return;
        }

        _aboutFontLoaded = true;
        const string path = @"C:\Windows\Fonts\bahnschrift.ttf";
        if (!File.Exists(path))
        {
            return;
        }

        var codepoints = AboutCodepoints;
        _aboutFont = Raylib.LoadFontEx(path, 64, codepoints, codepoints.Length);
        if (_aboutFont.GlyphCount <= 0)
        {
            return;
        }

        Raylib.SetTextureFilter(_aboutFont.Texture, TextureFilter.Bilinear);
        _aboutFontUsable = true;
    }

    private float MeasureAboutText(string text, int size)
    {
        return _aboutFontUsable
            ? Raylib.MeasureTextEx(_aboutFont, text, size, 2f).X
            : Raylib.MeasureText(text, size);
    }

    private void DrawAboutText(string text, float x, float y, int size, Color color)
    {
        if (_aboutFontUsable)
        {
            Raylib.DrawTextEx(_aboutFont, text, new System.Numerics.Vector2(x, y), size, 2f, color);
        }
        else
        {
            Raylib.DrawText(text, (int)x, (int)y, size, color);
        }
    }
}
