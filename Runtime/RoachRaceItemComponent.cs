using UnityEngine;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Playables;
using KINEMATION.CharacterAnimationSystem.Scripts.Runtime.Core;

namespace RoachRace.Interaction
{
    [RequireComponent(typeof(ItemInstance))]
    /// <summary>
    /// Base MonoBehaviour implementation of IRoachRaceItem.
    /// 
    /// Setup:
    /// - Derive from this (or RoachRaceItemBase) to implement usable items.
    /// - Place the item as a child GameObject under the player and register it via ItemInstance + PlayerItemRegistry.
    /// 
    /// Notes:
    /// - Inventory selection toggles items via SetVisibility (only the selected item is shown).
    /// - Default SetVisibility uses GameObject.SetActive; this means unselected items will have OnDisable/OnEnable called.
    ///   If your item needs to keep running while "hidden" (eg placement previews), override SetVisibility and
    ///   hide visuals without deactivating the GameObject.
    /// - By itself an item shows up in the inventory only if granted into slots by the server either via initialItems or pickups.
    /// </summary>
    public abstract class RoachRaceItemComponent : MonoBehaviour, IRoachRaceItem
    {
        public abstract Transform UseSource { get; }

        public CharacterAnimationSettings animationSettings;
        public Transform rightHandTarget;
        public Transform leftHandTarget;
        public AnimationAsset useAnimationAsset;
        CharacterAnimationComponent characterAnimationComponent;
        bool isEquipped = false;

        void Awake()
        {
            GetComponent<ItemInstance>().SetItemComponent(this);
            characterAnimationComponent = GetComponentInParent<CharacterAnimationComponent>();
        }

        public abstract void InitializeUseContext(int seed, int instigatorId, bool isServer, GameObject instigatorObject);

        public virtual void Equip() { }
        public virtual void OnEquipped() { 
            isEquipped = true;
        }
        
        public ItemInstance GetItemInstance()
        {
            return GetComponent<ItemInstance>();
        }

        public virtual Transform GetRightHandTarget() { return rightHandTarget; }
        public virtual Transform GetLeftHandTarget() { return leftHandTarget; }

        public virtual void UseStart()
        {
            if(!isEquipped) return;
               characterAnimationComponent.PlayAnimation(useAnimationAsset);
        }

        public virtual void UseStop() { }
        public virtual void Unequip() { }
        public virtual void OnUnequipped()
        {
            isEquipped = false;
        }

        public virtual void SetVisibility(bool isVisible) { 
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = isVisible;
            }
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.enabled = isVisible;
            }
        }
    }
}