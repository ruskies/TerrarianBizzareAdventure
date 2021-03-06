﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrarianBizzareAdventure.Players;
using TerrarianBizzareAdventure.Stands.Special.Developer.Webmilio;
using TerrarianBizzareAdventure.UserInterfaces.Elements;

namespace TerrarianBizzareAdventure.UserInterfaces.Special.RATM
{
    /// <summary>
    /// TO DO:
    /// Make UI triggered when stand's ability activated
    /// Make "Execute" use stand's ability
    /// </summary>
    public class RATMState : TBAUIState
    {
        private const float
            PANEL_WIDTH = 600,
            PANEL_HEIGHT = 400;

        private const int SAVE_SLOT_COUNT = 50;

        private Dictionary<PanelButton, InstantlyRunnable> _buttonsToRunnables = new Dictionary<PanelButton, InstantlyRunnable>();

        public override void OnInitialize()
        {
            CheckBoxes = new UICheckbox[]
                {
                    new UICheckbox(),
                    new UICheckbox(),
                    new UICheckbox()
                };
            CheckBoxTexts = new UIText[]
                {
                    new UIText("SEND NUDES TO CLIENTS"),
                    new UIText("SEND NUDES TO SERVER"),
                    new UIText("Idk something else")
                };

            CheckBoxes[0].Top.Set(40, 0);
            CheckBoxes[0].Left.Set(PANEL_WIDTH - 290, 0);
            CheckBoxes[1].Top.Set(80, 0);
            CheckBoxes[1].Left.Set(PANEL_WIDTH - 290, 0);
            CheckBoxes[2].Top.Set(120, 0);
            CheckBoxes[2].Left.Set(PANEL_WIDTH - 290, 0);

            CheckBoxTexts[0].Top.Set(50, 0);
            CheckBoxTexts[0].Left.Set(PANEL_WIDTH - 256, 0);
            CheckBoxTexts[1].Top.Set(90, 0);
            CheckBoxTexts[1].Left.Set(PANEL_WIDTH - 256, 0);
            CheckBoxTexts[2].Top.Set(130, 0);
            CheckBoxTexts[2].Left.Set(PANEL_WIDTH - 256, 0);

            MainPanel = new UIPanel();
            MainPanel.Width.Set(PANEL_WIDTH, 0);
            MainPanel.Height.Set(PANEL_HEIGHT, 0);
            MainPanel.SetPadding(0);
            MainPanel.VAlign = 0.5f;
            MainPanel.HAlign = 0.5f;

            var bgPanel = new UIPanel();
            bgPanel.Width.Set(250, 0);
            bgPanel.Height.Set(360, 0);
            bgPanel.SetPadding(0);
            bgPanel.VAlign = 0.5f;
            bgPanel.HAlign = 0.15f;

            ButtonGridScrollBar = new UIScrollbar();
            ButtonGridScrollBar.Width.Set(20, 0);
            ButtonGridScrollBar.Height.Set(350, 0);
            ButtonGridScrollBar.VAlign = 0.5f;
            ButtonGridScrollBar.HAlign = 0.05f;

            ButtonGrid = new UIGrid();
            ButtonGrid.Width.Set(250, 0);
            ButtonGrid.Height.Set(350, 0);
            ButtonGrid.SetScrollbar(ButtonGridScrollBar);
            ButtonGrid.Left.Set(5, 0);
            ButtonGrid.Top.Set(5, 0);

            for (int i = 0; i < SAVE_SLOT_COUNT; i++)
            {
                PanelButton button = new PanelButton("PLEASE COMPILE", 240);
                button.Left.Set(10, 0);

                button.OnClick += InstantlyRunnableButtonClick;

                ButtonGrid.Add(button);
            }

            bgPanel.Append(ButtonGrid);

            MainPanel.Append(bgPanel);

            CloseUIButton = new PanelButton("Close");
            CloseUIButton.Top.Set(PANEL_HEIGHT - 50, 0);
            CloseUIButton.Left.Set(PANEL_WIDTH - 100, 0);
            CloseUIButton.OnClick += new UIElement.MouseEvent(CloseUI);

            SendButton = new PanelButton("Execute", 80);
            SendButton.Top.Set(PANEL_HEIGHT - 50, 0);
            SendButton.Left.Set(PANEL_WIDTH - 184, 0);
            SendButton.OnClick += new UIElement.MouseEvent(ExecuteAction);

            MainPanel.Append(CloseUIButton);
            MainPanel.Append(SendButton);
            MainPanel.Append(ButtonGridScrollBar);

            foreach (UICheckbox check in CheckBoxes)
                MainPanel.Append(check);

            foreach (UIText text in CheckBoxTexts)
                MainPanel.Append(text);

            base.Append(MainPanel);
        }

        private void InstantlyRunnableButtonClick(UIMouseEvent evt, UIElement listeningelement)
        {
            PanelButton panelButton = evt.Target as PanelButton;

            if (panelButton == null)
                panelButton = evt.Target.Parent as PanelButton;

            if (!_buttonsToRunnables.ContainsKey(panelButton))
            {
                Main.NewText("No save data in selected slot!");
                return;
            }

            SelectedRunnable = _buttonsToRunnables[panelButton];
            LastInstantEnvironment.Select(SelectedRunnable);
            Main.NewText($"Selected save slot {ButtonGrid._items.IndexOf(panelButton) + 1} : {SelectedRunnable.GetType().Name}");
        }


        internal void GenerateButtons(InstantEnvironment instantEnvironment, List<InstantlyRunnable> instantlyRunnables)
        {
            /*_buttonsToRunnables.Clear();

            PanelButton GenerateButton(InstantlyRunnable instantlyRunnable)
            {
                PanelButton button = new PanelButton(instantlyRunnable.GetType().Name, 240);
                button.Left.Set(10, 0);

                button.OnClick += SelectInstantlyRunnable;

                return button;
            }

            foreach (InstantlyRunnable instantlyRunnable in instantlyRunnables)
            {
                PanelButton button = GenerateButton(instantlyRunnable);

                _buttonsToRunnables.Add(button, instantlyRunnable);
                ButtonGrid.Add(button);
            }*/

            _buttonsToRunnables.Clear();
            int saveSlotAffectedCount = instantlyRunnables.Count > SAVE_SLOT_COUNT ? SAVE_SLOT_COUNT : instantlyRunnables.Count;

            LastInstantEnvironment = instantEnvironment;

            for (int i = 0; i < saveSlotAffectedCount; i++)
            {
                InstantlyRunnable instantlyRunnable = instantlyRunnables[i];
                PanelButton panelButton = (ButtonGrid._items[i] as PanelButton);

                panelButton.NameText.SetText(instantlyRunnable.GetType().Name);
                _buttonsToRunnables.Add(panelButton, instantlyRunnable);
            }

            for (int i = saveSlotAffectedCount; i < SAVE_SLOT_COUNT; i++)
                (ButtonGrid._items[i] as PanelButton).NameText.SetText($"SAVE SLOT {i + 1}");
        }

        // TODO: change those two to something else
        public void ExecuteAction(UIMouseEvent evt, UIElement listeningElement)
        {
            if (SelectedRunnable == null)
            {
                Main.NewText("You must select a save slot!");
                return;
            }

            LastInstantEnvironment.Run(TBAPlayer.Get(Main.LocalPlayer));
        }

        public void CloseUI(UIMouseEvent evt, UIElement listeningElement)
        {
            SelectedRunnable = null;
            Visible = false;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);

            if (MainPanel.ContainsPoint(MousePosition))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            Recalculate();
            RecalculateChildren();
        }

        public PanelButton SendButton { get; private set; }

        public PanelButton CloseUIButton { get; private set; }

        public UIPanel MainPanel { get; private set; }

        public UIGrid ButtonGrid { get; private set; }

        public UIScrollbar ButtonGridScrollBar { get; private set; }

        public override bool Visible { get; set; }

        public UICheckbox[] CheckBoxes { get; private set; }

        public UIText[] CheckBoxTexts { get; private set; }


        public InstantEnvironment LastInstantEnvironment { get; private set; }
        public InstantlyRunnable SelectedRunnable { get; private set; }
    }
}
