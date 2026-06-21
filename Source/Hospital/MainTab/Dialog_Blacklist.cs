using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospital.MainTab
{
    /// <summary>
    /// Generic RimWorld-style picker for blacklisting defs (surgeries, diseases, factions, ...).
    /// Shows a searchable, scrollable list of checkboxes: checked = blacklisted (will not occur).
    /// State is read/written through delegates so it can route writes through Multiplayer sync methods.
    /// </summary>
    public class Dialog_Blacklist : Window
    {
        private readonly string title;
        private readonly List<Def> candidates;
        private readonly Func<Def, bool> isBlacklisted;
        private readonly Action<Def, bool> setBlacklisted;

        private readonly QuickSearchWidget searchWidget = new QuickSearchWidget();
        private Vector2 scrollPosition = Vector2.zero;

        private const float RowHeight = 28f;

        public Dialog_Blacklist(string title, IEnumerable<Def> candidates, Func<Def, bool> isBlacklisted, Action<Def, bool> setBlacklisted)
        {
            this.title = title;
            this.isBlacklisted = isBlacklisted;
            this.setBlacklisted = setBlacklisted;
            this.candidates = candidates
                .Where(d => d != null)
                .OrderBy(d => d.LabelCap.RawText)
                .ToList();

            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(500f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Text.Font = GameFont.Medium;
            float titleHeight = Text.CalcHeight(title, inRect.width);
            Widgets.Label(new Rect(0f, 0f, inRect.width, titleHeight), title);
            Text.Font = GameFont.Small;
            float y = titleHeight + 6f;

            // Hint
            Widgets.Label(new Rect(0f, y, inRect.width, 22f), "BlacklistHint".Translate());
            y += 26f;

            // Search box
            searchWidget.OnGUI(new Rect(0f, y, inRect.width, 28f));
            y += 34f;

            // Reserve space for the close button at the bottom (doCloseButton).
            float listBottom = inRect.height - (CloseButSize.y + 10f);
            Rect outRect = new Rect(0f, y, inRect.width, listBottom - y);

            List<Def> visible = candidates.Where(d => searchWidget.filter.Matches(d.LabelCap)).ToList();
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, visible.Count * RowHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float rowY = 0f;
            for (int i = 0; i < visible.Count; i++)
            {
                Def def = visible[i];
                Rect row = new Rect(0f, rowY, viewRect.width, RowHeight);
                if (i % 2 == 1) Widgets.DrawLightHighlight(row);
                if (Mouse.IsOver(row)) Widgets.DrawHighlight(row);

                bool wasBlacklisted = isBlacklisted(def);
                bool nowBlacklisted = wasBlacklisted;
                Widgets.CheckboxLabeled(row.ContractedBy(2f), def.LabelCap, ref nowBlacklisted);
                if (nowBlacklisted != wasBlacklisted)
                {
                    setBlacklisted(def, nowBlacklisted);
                }

                string tip = def.description;
                if (!tip.NullOrEmpty()) TooltipHandler.TipRegion(row, tip);

                rowY += RowHeight;
            }
            Widgets.EndScrollView();
        }
    }
}
