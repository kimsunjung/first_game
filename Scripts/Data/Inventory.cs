using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Data
{
    public class InventorySlot
    {
        public ItemData Item { get; set; }
        public int Quantity { get; set; }
        public int EnhancementLevel { get; set; } = 0;
        // v7: 인스턴스별 affix. 이번 PR에서는 빈 리스트로 유지 — 후속 PR이 드랍 시점에 채움.
        public List<ItemAffix> Affixes { get; set; } = new();
    }

    public class Inventory
    {
        public const int MaxSlots = 20;

        public List<InventorySlot> Slots { get; private set; } = new();

        // 장비 슬롯 (Equipment Slots)
        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }
        public ItemData EquippedAccessory { get; private set; }
        // 신규 부위별 슬롯 (반지 2개)
        public ItemData EquippedHelmet { get; internal set; }
        public ItemData EquippedBoots { get; internal set; }
        public ItemData EquippedNecklace { get; internal set; }
        public ItemData EquippedRing1 { get; internal set; }
        public ItemData EquippedRing2 { get; internal set; }
        public ItemData EquippedBracelet { get; internal set; }

        // 장착 중인 장비의 강화 수치.
        // 무기 전용 정책 — Armor/Accessory 강화는 폐기됐지만 SaveData/세이브 호환을 위해 필드는 남기고
        // 항상 0으로만 사용한다. EnhanceUI/Inventory 어디서도 더 이상 변경하지 않음.
        public int EquippedWeaponEnhancement { get; private set; } = 0;
        public int EquippedArmorEnhancement { get; private set; } = 0;
        public int EquippedAccessoryEnhancement { get; private set; } = 0;

        // 장신구(Ring/Necklace/Bracelet) 슬롯의 인스턴스별 affix.
        // Helmet/Boots는 이번 PR 범위에서 affix 미대상이라 페어 필드 없음.
        public List<ItemAffix> EquippedNecklaceAffixes { get; private set; } = new();
        public List<ItemAffix> EquippedRing1Affixes { get; private set; } = new();
        public List<ItemAffix> EquippedRing2Affixes { get; private set; } = new();
        public List<ItemAffix> EquippedBraceletAffixes { get; private set; } = new();

        // ─── 강화 헬퍼 ────────────────────────────────────────────
        // 무기 전용 정책. Armor/Accessory는 항상 0 — 호출처 변경 없이 자연스럽게 무효화된다.
        public static (int damage, int health, int defense) GetEnhancementBonuses(ItemData item, int level)
        {
            if (item == null || level <= 0 || item.Type != ItemType.Weapon) return (0, 0, 0);
            return (level * 2, 0, 0);
        }

        public static string GetEnhancedName(ItemData item, int enhancementLevel)
        {
            if (item == null) return "";
            return enhancementLevel > 0 ? $"{item.ItemName} +{enhancementLevel}" : item.ItemName;
        }

        public void NotifyChanged() => OnInventoryChanged?.Invoke();

        // 퀵슬롯 (Quick Slots - 4 slots)
        public ItemData[] QuickSlots { get; private set; } = new ItemData[4];

        // UI 갱신용 이벤트 (Events for UI updates)
        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemPickedUp;   // HUD 알림용
        public event Action OnEquipmentChanged;
        public event Action OnQuickSlotChanged;

        // --- 인벤토리 조작 ---

        /// <summary>
        /// 인벤토리에 아이템 추가.
        /// </summary>
        /// <param name="fireAcquired">
        /// true: 새 획득(필드 픽업/보상/구매)으로 간주해 OnItemPickedUp 발신.
        /// false: 복원/장착해제/롤백 등 내부 이동이라 픽업 토스트 미발신.
        /// </param>
        public bool AddItem(ItemData item, int amount = 1, int enhancementLevel = 0, bool fireAcquired = true, List<ItemAffix> affixes = null)
        {
            if (!CanAddItem(item, amount)) return false;

            // affix가 있는 인스턴스는 stack 불가 — 슬롯마다 고유 옵션 보존.
            bool hasAffixes = affixes != null && affixes.Count > 0;

            if (item.IsStackable && !hasAffixes)
            {
                int remaining = amount;
                int maxStack = Math.Max(1, item.MaxStack);

                foreach (var existing in Slots)
                {
                    if (!IsSameItem(existing.Item, item)) continue;
                    if (existing.Affixes != null && existing.Affixes.Count > 0) continue; // affix 있는 슬롯과 stack 금지

                    int space = maxStack - existing.Quantity;
                    if (space <= 0) continue;

                    int toAdd = Math.Min(remaining, space);
                    existing.Quantity += toAdd;
                    remaining -= toAdd;
                    if (remaining <= 0) break;
                }

                while (remaining > 0)
                {
                    int toAdd = Math.Min(remaining, maxStack);
                    Slots.Add(new InventorySlot
                    {
                        Item = item,
                        Quantity = toAdd,
                        EnhancementLevel = enhancementLevel
                    });
                    remaining -= toAdd;
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    Slots.Add(new InventorySlot
                    {
                        Item = item,
                        Quantity = 1,
                        EnhancementLevel = enhancementLevel,
                        Affixes = hasAffixes ? new List<ItemAffix>(affixes) : new List<ItemAffix>()
                    });
                }
            }

            OnInventoryChanged?.Invoke();
            if (fireAcquired) OnItemPickedUp?.Invoke(item);
            return true;
        }

        public bool CanAddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            if (!item.IsStackable)
                return Slots.Count + amount <= MaxSlots;

            int remaining = amount;
            int maxStack = Math.Max(1, item.MaxStack);

            foreach (var slot in Slots)
            {
                if (!IsSameItem(slot.Item, item)) continue;

                int space = maxStack - slot.Quantity;
                if (space <= 0) continue;

                remaining -= Math.Min(remaining, space);
                if (remaining <= 0) return true;
            }

            int emptySlots = MaxSlots - Slots.Count;
            int neededSlots = (remaining + maxStack - 1) / maxStack;
            return neededSlots <= emptySlots;
        }

        private static bool IsSameItem(ItemData a, ItemData b)
        {
            if (a == null || b == null) return false;
            if (!string.IsNullOrEmpty(a.ResourcePath) && !string.IsNullOrEmpty(b.ResourcePath))
                return a.ResourcePath == b.ResourcePath;
            return ReferenceEquals(a, b);
        }

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            Slots[slotIndex].Quantity = Math.Max(0, Slots[slotIndex].Quantity - amount);
            if (Slots[slotIndex].Quantity <= 0)
                Slots.RemoveAt(slotIndex);
            OnInventoryChanged?.Invoke();
        }

        // --- 아이템 사용 ---

        public void UseItem(int slotIndex, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.Item.Type == ItemType.Consumable)
            {
                switch (slot.Item.UseEffect)
                {
                    case ItemUseEffect.ReturnToTown:
                        // 귀환 주문서 트랜잭션 — 막아야 할 4가지.
                        // 1) RemoveItem이 OnInventoryChanged → RequestAutoSave를 트리거해 중간 상태가
                        //    디스크에 박히는 것 → suspendAutoSave (GameTransaction 기본 true)
                        // 2) RemoveItem이 만든 빈 슬롯을 TryClaim이 가로채는 것 + TryClaim의 직접
                        //    SaveGame이 suspend를 우회하는 것 → suspendPendingClaims (기본 true)
                        // 3) AddItem 복구 실패 시 스크롤 손실 → 반환값 체크 + PendingReward fallback
                        // 4) Teleport 성공 후 dispose에서 TryClaim이 실행되면 deferred 큐 상태에서
                        //    BuildSaveData가 old CurrentScene을 캡처해 디스크를 덮는 것 →
                        //    SetClaimAfterDispose(false)로 새 씬에 위임
                        if (!TownReturnHandler.CanUse()) return;
                        var consumed = slot.Item;
                        int consumedLevel = slot.EnhancementLevel;
                        using (var tx = GameTransaction.Begin())
                        {
                            RemoveItem(slotIndex, 1);
                            AudioManager.Instance?.PlaySFX("potion_use.wav");
                            bool teleportSucceeded = TownReturnHandler.Teleport();
                            if (teleportSucceeded)
                            {
                                tx.SetClaimAfterDispose(false); // 새 씬 _Ready → LoadFromSaveData → TryClaim 위임
                            }
                            else
                            {
                                bool restored = AddItem(consumed, 1, consumedLevel, fireAcquired: false);
                                if (!restored)
                                {
                                    GameManager.Instance?.AddPendingReward(consumed, 1, consumedLevel);
                                    GD.PrintErr("귀환 실패 + 인벤 복구 실패 — 주문서를 보류 보상에 보관");
                                }
                                else
                                {
                                    GD.Print("귀환 실패 — 주문서를 돌려받았습니다.");
                                }
                            }
                        }
                        return;

                    case ItemUseEffect.Heal:
                        target.Heal(slot.Item.HealAmount);
                        GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
                        AudioManager.Instance?.PlaySFX("potion_use.wav");
                        RemoveItem(slotIndex, 1);
                        return;

                    case ItemUseEffect.None:
                    default:
                        return;
                }
            }
            else if (slot.Item.Type.IsEquipment() && slot.Item.Type != ItemType.SkillBook)
            {
                EquipItem(slotIndex, target);
            }
            else if (slot.Item.Type == ItemType.SkillBook)
            {
                if (slot.Item.LearnedSkill == null) return;
                bool learned = target.LearnSkill(slot.Item.LearnedSkill);
                if (learned)
                {
                    AudioManager.Instance?.PlaySFX("skill_learn.wav");
                    RemoveItem(slotIndex, 1);
                }
            }
        }

        // --- 장비 장착/해제 ---

        public void EquipItem(int slotIndex, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;
            int enhLevel = slot.EnhancementLevel;

            // 기존 장비를 해당 슬롯 자리에 in-place로 되돌려놓음으로써 가방이 꽉 차도
            // 교체 가능하게 한다. UnequipXxx → AddItem 경로는 빈 슬롯을 요구해서
            // MaxSlots 도달 시 거부됐음.
            if (item.Type == ItemType.Weapon)
            {
                var prevItem = EquippedWeapon;
                int prevEnh = EquippedWeaponEnhancement;
                if (prevItem != null)
                {
                    var (pdmg, _, _) = GetEnhancementBonuses(prevItem, prevEnh);
                    target.ModifyBaseDamage(-(prevItem.BonusDamage + pdmg));
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = prevEnh;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedWeapon = item;
                EquippedWeaponEnhancement = enhLevel;
                var (dmg, _, _) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyBaseDamage(item.BonusDamage + dmg);
            }
            else if (item.Type == ItemType.Armor)
            {
                var prevItem = EquippedArmor;
                if (prevItem != null)
                {
                    target.ModifyMaxHealth(-prevItem.BonusMaxHealth);
                    target.ModifyDefense(-prevItem.BonusDefense);
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = 0;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedArmor = item;
                target.ModifyMaxHealth(item.BonusMaxHealth);
                target.ModifyDefense(item.BonusDefense);
            }
            else if (item.Type == ItemType.Accessory)
            {
                var prevItem = EquippedAccessory;
                if (prevItem != null)
                {
                    target.ModifyDefense(-prevItem.BonusDefense);
                    if (prevItem.BonusDamage > 0) target.ModifyBaseDamage(-prevItem.BonusDamage);
                    if (prevItem.BonusMaxHealth > 0) target.ModifyMaxHealth(-prevItem.BonusMaxHealth);
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = 0;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedAccessory = item;
                target.ModifyDefense(item.BonusDefense);
                if (item.BonusDamage > 0) target.ModifyBaseDamage(item.BonusDamage);
                if (item.BonusMaxHealth > 0) target.ModifyMaxHealth(item.BonusMaxHealth);
            }
            else if (IsExtraEquipType(item.Type))
            {
                EquipExtraInternal(slotIndex, item, ResolveExtraSlot(item.Type), target);
                OnInventoryChanged?.Invoke();
                OnEquipmentChanged?.Invoke();
                AudioManager.Instance?.PlaySFX("equip.wav");
                GD.Print($"{GetEnhancedName(item, enhLevel)} 장착!");
                return;
            }

            OnInventoryChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{GetEnhancedName(item, enhLevel)} 장착!");
        }

        /// <summary>반지처럼 같은 ItemType이 두 슬롯(Ring1/Ring2)을 가질 때 사용자가
        /// 어느 슬롯에 장착/교체할지 직접 지정. ResolveExtraSlot이 자동으로 빈 칸을 고르는
        /// 기본 EquipItem과 달리, 둘 다 차 있어도 지정한 슬롯으로 교체 가능.</summary>
        public void EquipItemToSlot(int slotIndex, ExtraSlot targetSlot, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;
            if (!IsExtraEquipType(item.Type)) return;
            // 슬롯 유형이 아이템과 맞는지 검증 (Ring → Ring1/Ring2만 허용 등)
            if (!IsSlotCompatible(item.Type, targetSlot)) return;
            EquipExtraInternal(slotIndex, item, targetSlot, target);
            OnInventoryChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{item.ItemName} 장착!");
        }

        private static bool IsSlotCompatible(ItemType t, ExtraSlot s) => (t, s) switch
        {
            (ItemType.Helmet, ExtraSlot.Helmet) => true,
            (ItemType.Boots, ExtraSlot.Boots) => true,
            (ItemType.Necklace, ExtraSlot.Necklace) => true,
            (ItemType.Bracelet, ExtraSlot.Bracelet) => true,
            (ItemType.Ring, ExtraSlot.Ring1) => true,
            (ItemType.Ring, ExtraSlot.Ring2) => true,
            _ => false
        };

        private void EquipExtraInternal(int slotIndex, ItemData item, ExtraSlot slotKey, IEquipTarget target)
        {
            var slot = Slots[slotIndex];
            // 인벤 슬롯에서 in-place 교체 — slot.Affixes 참조가 prev로 덮이기 전에 캡처.
            var newAffixes = slot.Affixes != null ? new List<ItemAffix>(slot.Affixes) : new List<ItemAffix>();
            var prev = GetExtraSlot(slotKey);
            var prevAffixes = GetExtraAffixes(slotKey);
            if (prev != null)
            {
                ApplyItemBonuses(prev, target, -1);
                ApplyAffixBonuses(prevAffixes, target, -1);
                slot.Item = prev;
                slot.Quantity = 1;
                slot.EnhancementLevel = 0;
                slot.Affixes = prevAffixes != null ? new List<ItemAffix>(prevAffixes) : new List<ItemAffix>();
            }
            else
            {
                Slots.RemoveAt(slotIndex);
            }
            SetExtraSlot(slotKey, item);
            SetExtraAffixes(slotKey, newAffixes);
            ApplyItemBonuses(item, target, +1);
            ApplyAffixBonuses(GetExtraAffixes(slotKey), target, +1);
        }

        public bool UnequipWeapon(IEquipTarget target)
        {
            if (EquippedWeapon == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 무기를 해제할 수 없습니다!");
                return false;
            }

            using (GameTransaction.Begin())
            {
                var (dmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
                target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + dmg));
                bool added = AddItem(EquippedWeapon, 1, EquippedWeaponEnhancement, fireAcquired: false);
                if (!added) return false;

                EquippedWeapon = null;
                EquippedWeaponEnhancement = 0;
                OnEquipmentChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        public bool UnequipArmor(IEquipTarget target)
        {
            if (EquippedArmor == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 방어구를 해제할 수 없습니다!");
                return false;
            }

            using (GameTransaction.Begin())
            {
                target.ModifyMaxHealth(-EquippedArmor.BonusMaxHealth);
                target.ModifyDefense(-EquippedArmor.BonusDefense);
                bool added = AddItem(EquippedArmor, 1, 0, fireAcquired: false);
                if (!added) return false;

                EquippedArmor = null;
                OnEquipmentChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        public bool UnequipAccessory(IEquipTarget target)
        {
            if (EquippedAccessory == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 악세서리를 해제할 수 없습니다!");
                return false;
            }

            using (GameTransaction.Begin())
            {
                target.ModifyDefense(-EquippedAccessory.BonusDefense);
                if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(-EquippedAccessory.BonusDamage);
                if (EquippedAccessory.BonusMaxHealth > 0) target.ModifyMaxHealth(-EquippedAccessory.BonusMaxHealth);
                bool added = AddItem(EquippedAccessory, 1, 0, fireAcquired: false);
                if (!added) return false;

                EquippedAccessory = null;
                OnEquipmentChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        // --- 신규 부위별 장비 슬롯 헬퍼 ---

        /// <summary>모자/신발/목걸이/반지/팔찌처럼 강화 미지원 신규 부위인지.</summary>
        public static bool IsExtraEquipType(ItemType t) =>
            t == ItemType.Helmet || t == ItemType.Boots || t == ItemType.Necklace ||
            t == ItemType.Ring || t == ItemType.Bracelet;

        public enum ExtraSlot { Helmet, Boots, Necklace, Ring1, Ring2, Bracelet }

        /// <summary>아이템 타입을 어떤 부위 슬롯에 둘지 결정. 반지는 빈 슬롯 우선.</summary>
        private ExtraSlot ResolveExtraSlot(ItemType t) => t switch
        {
            ItemType.Helmet => ExtraSlot.Helmet,
            ItemType.Boots => ExtraSlot.Boots,
            ItemType.Necklace => ExtraSlot.Necklace,
            ItemType.Bracelet => ExtraSlot.Bracelet,
            ItemType.Ring => EquippedRing1 == null ? ExtraSlot.Ring1 : ExtraSlot.Ring2,
            _ => ExtraSlot.Helmet
        };

        public ItemData GetExtraSlot(ExtraSlot s) => s switch
        {
            ExtraSlot.Helmet => EquippedHelmet,
            ExtraSlot.Boots => EquippedBoots,
            ExtraSlot.Necklace => EquippedNecklace,
            ExtraSlot.Ring1 => EquippedRing1,
            ExtraSlot.Ring2 => EquippedRing2,
            ExtraSlot.Bracelet => EquippedBracelet,
            _ => null
        };

        private void SetExtraSlot(ExtraSlot s, ItemData item)
        {
            switch (s)
            {
                case ExtraSlot.Helmet: EquippedHelmet = item; break;
                case ExtraSlot.Boots: EquippedBoots = item; break;
                case ExtraSlot.Necklace: EquippedNecklace = item; break;
                case ExtraSlot.Ring1: EquippedRing1 = item; break;
                case ExtraSlot.Ring2: EquippedRing2 = item; break;
                case ExtraSlot.Bracelet: EquippedBracelet = item; break;
            }
        }

        /// <summary>장신구 슬롯의 affix 페어. Helmet/Boots는 null 반환(미대상).</summary>
        public List<ItemAffix> GetExtraAffixes(ExtraSlot s) => s switch
        {
            ExtraSlot.Necklace => EquippedNecklaceAffixes,
            ExtraSlot.Ring1 => EquippedRing1Affixes,
            ExtraSlot.Ring2 => EquippedRing2Affixes,
            ExtraSlot.Bracelet => EquippedBraceletAffixes,
            _ => null
        };

        private void SetExtraAffixes(ExtraSlot s, List<ItemAffix> affixes)
        {
            var copy = affixes != null ? new List<ItemAffix>(affixes) : new List<ItemAffix>();
            switch (s)
            {
                case ExtraSlot.Necklace: EquippedNecklaceAffixes = copy; break;
                case ExtraSlot.Ring1:    EquippedRing1Affixes    = copy; break;
                case ExtraSlot.Ring2:    EquippedRing2Affixes    = copy; break;
                case ExtraSlot.Bracelet: EquippedBraceletAffixes = copy; break;
                // Helmet/Boots는 affix 미대상 — 무시.
            }
        }

        /// <summary>장비 보너스 일괄 적용 (sign=+1 장착, -1 해제).</summary>
        private static void ApplyItemBonuses(ItemData item, IEquipTarget target, int sign)
        {
            if (item.BonusDamage != 0) target.ModifyBaseDamage(sign * item.BonusDamage);
            if (item.BonusMaxHealth != 0) target.ModifyMaxHealth(sign * item.BonusMaxHealth);
            if (item.BonusDefense != 0) target.ModifyDefense(sign * item.BonusDefense);
            if (item.BonusMaxMp != 0) target.ModifyMaxMp(sign * item.BonusMaxMp);
            if (item.BonusCritRate != 0f) target.ModifyCritRate(sign * item.BonusCritRate);
            if (item.BonusMoveSpeed != 0f) target.ModifyMoveSpeed(sign * item.BonusMoveSpeed);
        }

        /// <summary>affix 합산 적용 (sign=+1 장착, -1 해제). 같은 type이 여러 개면 모두 누적.</summary>
        private static void ApplyAffixBonuses(List<ItemAffix> affixes, IEquipTarget target, int sign)
        {
            if (affixes == null || affixes.Count == 0) return;
            foreach (var a in affixes)
            {
                switch (a.Type)
                {
                    case ItemAffixType.BonusDamage:    target.ModifyBaseDamage(sign * (int)a.Value); break;
                    case ItemAffixType.BonusDefense:   target.ModifyDefense(sign * (int)a.Value); break;
                    case ItemAffixType.BonusMaxHealth: target.ModifyMaxHealth(sign * (int)a.Value); break;
                    case ItemAffixType.BonusMaxMp:     target.ModifyMaxMp(sign * (int)a.Value); break;
                    case ItemAffixType.BonusCritRate:  target.ModifyCritRate(sign * a.Value); break;
                    case ItemAffixType.BonusMoveSpeed: target.ModifyMoveSpeed(sign * a.Value); break;
                }
            }
        }

        /// <summary>세이브에서 신규 부위별 슬롯 복원. 빈 path는 무시.
        /// affixes는 Necklace/Ring/Bracelet 슬롯에만 의미 있음. Helmet/Boots는 null 전달.</summary>
        public void RestoreExtraSlot(ExtraSlot slot, string itemPath, List<ItemAffix> affixes, IEquipTarget target)
        {
            if (string.IsNullOrEmpty(itemPath)) return;
            var item = GD.Load<ItemData>(itemPath);
            if (item == null) return;

            SetExtraSlot(slot, item);
            SetExtraAffixes(slot, affixes);
            ApplyItemBonuses(item, target, +1);
            ApplyAffixBonuses(GetExtraAffixes(slot), target, +1);
            OnEquipmentChanged?.Invoke();
        }

        public bool UnequipExtra(ExtraSlot slot, IEquipTarget target)
        {
            var item = GetExtraSlot(slot);
            if (item == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 장비를 해제할 수 없습니다!");
                return false;
            }

            // 트랜잭션 격리 — AddItem의 OnInventoryChanged 자동저장이 끼면 "스탯은 빠졌는데
            // 장비 슬롯은 아직 차 있는" 중간 상태가 디스크에 박힌다. dispose 후 한 번에 영속화.
            using (GameTransaction.Begin())
            {
                var affixes = GetExtraAffixes(slot);
                ApplyItemBonuses(item, target, -1);
                ApplyAffixBonuses(affixes, target, -1);
                bool added = AddItem(item, 1, 0, fireAcquired: false, affixes);
                if (!added) return false;

                SetExtraSlot(slot, null);
                SetExtraAffixes(slot, new List<ItemAffix>());
                OnEquipmentChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        // --- 퀵슬롯 ---

        public void AssignQuickSlot(int quickSlotIndex, ItemData item)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;
            QuickSlots[quickSlotIndex] = item;
            OnQuickSlotChanged?.Invoke();
            GD.Print($"퀵슬롯 {quickSlotIndex + 1}번에 {item.ItemName} 등록됨.");
        }

        public void UseQuickSlot(int quickSlotIndex, IEquipTarget target)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;
            var item = QuickSlots[quickSlotIndex];
            if (item == null) return;

            // ResourcePath 비교 — Resource 캐시가 다른 인스턴스를 반환하더라도 동일 아이템으로 매칭.
            int slotIndex = Slots.FindIndex(s => IsSameItem(s.Item, item));
            if (slotIndex != -1)
            {
                UseItem(slotIndex, target);
            }
            else
            {
                GD.Print($"{item.ItemName}이(가) 인벤토리에 없습니다!");
            }
        }

        /// <summary>세이브 복원용 일괄 퀵슬롯 적용. OnQuickSlotChanged 한 번만 발신.</summary>
        public void RestoreQuickSlots(ItemData[] items)
        {
            if (items == null) return;
            int count = Math.Min(items.Length, QuickSlots.Length);
            for (int i = 0; i < count; i++)
                QuickSlots[i] = items[i];
            OnQuickSlotChanged?.Invoke();
        }

        // --- 크래프팅 지원 ---

        public int CountItem(ItemData item)
        {
            int count = 0;
            foreach (var slot in Slots)
            {
                if (slot.Item.ResourcePath == item.ResourcePath)
                    count += slot.Quantity;
            }
            return count;
        }

        public bool HasItems(ItemData item, int amount)
        {
            return CountItem(item) >= amount;
        }

        public bool ConsumeItems(ItemData item, int amount)
        {
            if (!HasItems(item, amount)) return false;

            int remaining = amount;
            for (int i = Slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (Slots[i].Item.ResourcePath != item.ResourcePath) continue;

                if (Slots[i].Quantity <= remaining)
                {
                    remaining -= Slots[i].Quantity;
                    Slots.RemoveAt(i);
                }
                else
                {
                    Slots[i].Quantity -= remaining;
                    remaining = 0;
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        // --- 강화 지원 (EnhanceUI용) ---

        /// <summary>장착 중인 무기의 강화 수치 변경 (보너스 재계산 포함). 무기 외 ItemType은 무시.</summary>
        public void SetEquippedEnhancement(ItemType type, int newLevel, IEquipTarget target)
        {
            if (type != ItemType.Weapon || EquippedWeapon == null) return;
            var (oldWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-oldWDmg);
            EquippedWeaponEnhancement = newLevel;
            var (newWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, newLevel);
            target.ModifyBaseDamage(newWDmg);
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>장착 중인 무기 파괴 (강화 실패 시). 보너스 제거 후 null로. 무기 외 ItemType은 무시.</summary>
        public void DestroyEquippedItem(ItemType type, IEquipTarget target)
        {
            if (type != ItemType.Weapon || EquippedWeapon == null) return;
            var (wDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + wDmg));
            EquippedWeapon = null;
            EquippedWeaponEnhancement = 0;
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>
        /// 세이브 데이터의 모든 장비 슬롯을 일괄 복원하는 단일 진입점.
        /// Weapon/Armor/Accessory + Helmet/Boots/Necklace/Ring1/Ring2/Bracelet 9개 슬롯과
        /// v3→v4 Accessory 재분류 마이그까지 한 번에 처리한다.
        /// </summary>
        /// <returns>마이그 결과 (호출자가 사용자 알림에 활용 가능)</returns>
        public EquipmentRestoreReport RestoreFromSaveData(SaveData data, IEquipTarget target)
        {
            var report = new EquipmentRestoreReport();

            var loadedWeapon    = LoadItemPath(data.EquippedWeaponPath);
            var loadedArmor     = LoadItemPath(data.EquippedArmorPath);
            var loadedAccessory = LoadItemPath(data.EquippedAccessoryPath);

            string migrateNecklacePath = data.EquippedNecklacePath;
            string migrateRing1Path = data.EquippedRing1Path;

            // v3→v4: 구 Accessory 슬롯 아이템이 Necklace/Ring으로 재분류된 경우.
            // 신규 슬롯은 강화 미지원 — 강화 +N은 인벤토리로 반환해 보존.
            if (loadedAccessory != null &&
                (loadedAccessory.Type == ItemType.Necklace || loadedAccessory.Type == ItemType.Ring))
            {
                if (data.EquippedAccessoryEnhancement > 0)
                {
                    report.MigratedItem = loadedAccessory;
                    report.MigratedEnhancement = data.EquippedAccessoryEnhancement;
                    if (AddItem(loadedAccessory, 1, data.EquippedAccessoryEnhancement, fireAcquired: false))
                    {
                        report.MigratedToInventory = true;
                        loadedAccessory = null;
                    }
                    else
                    {
                        report.MigratedToInventory = false; // 인벤 가득 — 강화 손실 강제 장착
                    }
                }

                if (loadedAccessory != null)
                {
                    if (loadedAccessory.Type == ItemType.Necklace && string.IsNullOrEmpty(migrateNecklacePath))
                    {
                        migrateNecklacePath = data.EquippedAccessoryPath;
                        loadedAccessory = null;
                    }
                    else if (loadedAccessory.Type == ItemType.Ring && string.IsNullOrEmpty(migrateRing1Path))
                    {
                        migrateRing1Path = data.EquippedAccessoryPath;
                        loadedAccessory = null;
                    }
                }
            }

            // 무기 전용 정책: data.EquippedArmorEnhancement / EquippedAccessoryEnhancement는
            // 의도적으로 무시한다. v6 이하 세이브에 비정상적으로 강화 +N이 박혀 있어도 0으로 클램프.
            RestoreEquipment(loadedWeapon, loadedArmor, target, loadedAccessory,
                data.EquippedWeaponEnhancement);

            RestoreExtraSlot(ExtraSlot.Helmet, data.EquippedHelmetPath, null, target); // Helmet은 affix 미대상
            RestoreExtraSlot(ExtraSlot.Boots, data.EquippedBootsPath, null, target);  // Boots는 affix 미대상
            RestoreExtraSlot(ExtraSlot.Necklace, migrateNecklacePath, data.EquippedNecklaceAffixes, target);
            RestoreExtraSlot(ExtraSlot.Ring1, migrateRing1Path, data.EquippedRing1Affixes, target);
            RestoreExtraSlot(ExtraSlot.Ring2, data.EquippedRing2Path, data.EquippedRing2Affixes, target);
            RestoreExtraSlot(ExtraSlot.Bracelet, data.EquippedBraceletPath, data.EquippedBraceletAffixes, target);

            return report;
        }

        private static ItemData LoadItemPath(string path)
            => string.IsNullOrEmpty(path) ? null : GD.Load<ItemData>(path);

        public class EquipmentRestoreReport
        {
            /// <summary>v3→v4 마이그로 처리된 강화 Accessory. null이면 마이그 발생 안 함.</summary>
            public ItemData MigratedItem;
            public int MigratedEnhancement;
            /// <summary>true=인벤토리로 안전 반환, false=인벤 가득해 강화 손실하며 신규 슬롯에 강제 장착.</summary>
            public bool MigratedToInventory;
        }

        // 세이브/로드 시 장비 복원 (RestoreFromSaveData 내부에서 호출).
        // 강화는 무기 전용 — Armor/Accessory에는 강화 수치 적용 안 함.
        public void RestoreEquipment(ItemData weapon, ItemData armor, IEquipTarget target,
            ItemData accessory = null,
            int weaponEnhance = 0)
        {
            if (weapon != null)
            {
                EquippedWeapon = weapon;
                EquippedWeaponEnhancement = weaponEnhance;
                var (dmg, _, _) = GetEnhancementBonuses(weapon, weaponEnhance);
                target.ModifyBaseDamage(weapon.BonusDamage + dmg);
            }
            if (armor != null)
            {
                EquippedArmor = armor;
                target.ModifyMaxHealth(armor.BonusMaxHealth);
                target.ModifyDefense(armor.BonusDefense);
            }
            if (accessory != null)
            {
                EquippedAccessory = accessory;
                target.ModifyDefense(accessory.BonusDefense);
                if (accessory.BonusDamage > 0) target.ModifyBaseDamage(accessory.BonusDamage);
                if (accessory.BonusMaxHealth > 0) target.ModifyMaxHealth(accessory.BonusMaxHealth);
            }
            OnEquipmentChanged?.Invoke();
        }
    }
}
