using System.Collections.Generic;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using AOClient.Player.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Spells
{
    public sealed class SpellsUI : MonoBehaviour
    {
        [SerializeField] private Button showSpellsButton, castSpellButton, showInfoButton, moveSpellUpButton, moveSpellDownButton;
        [SerializeField] private Transform spellsContainer;

        private SpellSlotUI selectedSpell;

        private readonly List<SpellSlotUI> spellsSlots = new();

        private void Start()
        {
            showSpellsButton.onClick.AddListener(ShowSpells);
            castSpellButton.onClick.AddListener(CastSpell);
            showInfoButton.onClick.AddListener(ShowSpellInfo);
            moveSpellUpButton.onClick.AddListener(() => MoveSpell(true));
            moveSpellDownButton.onClick.AddListener(() => MoveSpell(false));

            foreach (Transform child in spellsContainer)
            {
                var spellSlot = child.gameObject.GetComponent<SpellSlotUI>();
                spellSlot.Button.onClick.AddListener(() => ClickSpell(spellSlot.SlotId));
                spellsSlots.Add(spellSlot);
            }

            selectedSpell = spellsSlots[0];
            ClickSpell(0);

            HideSpells();
        }

        public void UpdateSpells(Spell spell)
        {
            spellsSlots[spell.Slot].SpellNameText.text = spell.Name;
        }

        public void HideSpells()
        {
            gameObject.SetActive(false);
        }

        public void SpellMoved(byte slotOne, byte slotTwo)
        {
            SpellSlotUI slotA = spellsSlots[slotOne];
            SpellSlotUI slotB = spellsSlots[slotTwo];

            //Swap names
            (slotA.SpellNameText.text, slotB.SpellNameText.text) = (slotB.SpellNameText.text, slotA.SpellNameText.text);

            //Change highlight
            selectedSpell.HighlightImage.enabled = false;
            slotB.HighlightImage.enabled = true;

            //Change selected id
            selectedSpell = slotB;
        }

        private void MoveSpell(bool up)
        {
            PacketSender.MovePlayerSpell(selectedSpell.SlotId, up);
        }

        public void ClickSpell(byte id)
        {
            SpellSlotUI newSelectedSlot = spellsSlots[id];

            selectedSpell.HighlightImage.enabled = false;
            newSelectedSlot.HighlightImage.enabled = true;
            selectedSpell = newSelectedSlot;
        }

        private void ShowSpells()
        {
            gameObject.SetActive(true);
        }

        private void CastSpell()
        {
            PacketSender.PlayerSelectedSpell(selectedSpell.SlotId);
        }

        private void ShowSpellInfo()
        {
            ConsoleUI console = UIManager.GameUI.Console;
            var spell = GameManager.Instance.LocalPlayer.GetSpellAtIndex(selectedSpell.SlotId);

            if (!string.IsNullOrEmpty(spell.Name))
            {
                console.WriteLine("%%%%%%%%%% INFORMACIÓN DEL HECHIZO %%%%%%%%%%");
                console.WriteLine($"Nombre: {spell.Name}.");
                console.WriteLine($"Descripción: {spell.Description}");
                console.WriteLine($"Skill requerido: {spell.MinSkill}.");
                console.WriteLine($"Mana necesario: {spell.ManaRequired}.");
                console.WriteLine($"Energía necesaria: {spell.StamRequired}.");
                console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            }
        }
    }
}
